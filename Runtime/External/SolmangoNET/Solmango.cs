using OneOf;
using SolmangoNET.Exceptions;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolmangoNET
{
    public static class Solmango
    {
        /// <summary>
        ///   Get the associated token account
        /// </summary>
        /// <param name="rpcClient"> </param>
        /// <param name="owner"> </param>
        /// <param name="tokenMint"> </param>
        /// <returns> The associated token account, null if not found </returns>
        public static async Task<PublicKey> GetAssociatedTokenAccount(IRpcClient rpcClient, string owner, string tokenMint)
        {
            var associatedAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(new PublicKey(owner), new PublicKey(tokenMint));
            var res = await rpcClient.GetTokenAccountBalanceAsync(associatedAccount);
            return res.WasRequestSuccessfullyHandled ? associatedAccount : null;
        }

        /// <summary>
        ///   Sends a multiple transactions in batch to greatly speed up the process.
        ///   <para> Use unbounded endpoints, look at <see href="https://www.genesysgo.com/"/> </para>
        /// </summary>
        /// <param name="rpcClient"> </param>
        /// <param name="transactions"> All the transactions </param>
        /// <param name="batchSize"> The amount of requests in a single batch </param>
        /// <returns> A list of tuples identifying the transaction and whether it has been succesfully sent to the cluster </returns>
        public static async Task<List<(string transaction, bool success)>> SendTransactionBatch(IRpcClient rpcClient, List<byte[]> transactions, int batchSize = 100)
        {
            var batcher = new SolanaRpcBatchWithCallbacks(rpcClient);
            batcher.AutoExecute(BatchAutoExecuteMode.ExecuteWithCallbackFailures, batchSize);

            var results = new List<(string transaction, bool success)>();
            foreach (var transaction in transactions)
            {
                batcher.SendTransaction(transaction, false, Commitment.Finalized, (res, ex) => results.Add((res, ex is not null)));
            }
            // This call is actually blocking, so the function needs to be async in order not to block
            batcher.Flush();

            // Removes compiler warning
            await Task.CompletedTask;

            // Return a list of all transaction with a bool identifying if they are successful or not. Returning the transactions strings
            // allows to subscribe to their confirmation
            return results;
        }

        /// <summary>
        ///   Builds a transaction to send an SPL token.
        ///   <para> Use it with <see cref="SendTransactionBatch(IRpcClient, List{byte[]}, int)"/> in order to batch multiple transactions. </para>
        /// </summary>
        /// <param name="rpcClient"> </param>
        /// <param name="sender"> </param>
        /// <param name="receiver"> </param>
        /// <param name="tokenMint"> </param>
        /// <param name="amount"> </param>
        /// <returns> The transaction hash upon success </returns>
        public static async Task<OneOf<byte[], Exception>> BuildSendSplTokenTransaction(IRpcClient rpcClient, Account sender, string receiver, string tokenMint, double amount)
        {
            // Get the blockhash
            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            if (blockHash is null) return new Exception("BlockHash can't be null");
            if (!blockHash.WasRequestSuccessfullyHandled) return new SolmangoRpcException(blockHash.Reason, blockHash.ServerErrorCode);

            // Get the sender ata
            var fromAta = await GetAssociatedTokenAccount(rpcClient, sender.PublicKey, tokenMint);
            if (fromAta is null) return new Exception("Sender ata can't be null");

            // Get the token decimals and the actual amount
            var res = await rpcClient.GetTokenSupplyAsync(tokenMint);
            if (res is null) return new Exception("Token decimal can't be null");
            if (!res.WasRequestSuccessfullyHandled) return new SolmangoRpcException(res.Reason, res.ServerErrorCode);

            var actualAmount = (ulong)(amount * Math.Pow(10, res.Result.Value.Decimals));

            byte[] transaction;
            // Get or create the receiver ata
            var toAta = await GetAssociatedTokenAccount(rpcClient, receiver, tokenMint);
            if (toAta is null)
            {
                toAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(new PublicKey(receiver), new PublicKey(tokenMint));
                transaction = new TransactionBuilder()
                   .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                   .SetFeePayer(sender)
                   .AddInstruction(
                    AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                       sender,
                       new PublicKey(receiver),
                       new PublicKey(tokenMint)))
                   .AddInstruction(TokenProgram.Transfer(
                       fromAta,
                       toAta,
                       actualAmount,
                       sender.PublicKey))
                   .Build(sender);
            }
            else
            {
                transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(sender)
                .AddInstruction(TokenProgram.Transfer(
                    fromAta,
                    toAta,
                    actualAmount,
                    sender.PublicKey))
                .Build(sender);
            }

            return transaction;
        }

        /// <summary>
        ///   Sends a custom SPL token. Handles the associated token account
        /// </summary>
        /// <param name="rpcClient"> </param>
        /// <param name="receiver"> </param>
        /// <param name="sender"> </param>
        /// <param name="tokenMint"> </param>
        /// <param name="amount"> The amount in Sol to send </param>
        /// <returns> The signature of the transaction upon success </returns>
        public static async Task<OneOf<string, Exception>> SendSplToken(IRpcClient rpcClient, Account sender, string receiver, string tokenMint, double amount)
        {
            var res = await BuildSendSplTokenTransaction(rpcClient, sender, receiver, tokenMint, amount);
            if (res.TryPickT1(out var ex, out var transaction)) return ex;
            var result = await rpcClient.SendTransactionAsync(transaction);
            // Return the signature of the transaction, so that it is possible to subscribe to its confirmation
            return result is null ?
                new Exception("Transaction signature can't be null") :
                    result.WasRequestSuccessfullyHandled ?
                        result.Result :
                        new SolmangoRpcException(result.Reason, result.ServerErrorCode);
        }
    }
}
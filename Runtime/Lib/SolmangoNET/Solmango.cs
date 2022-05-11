using OneOf;
using SolmangoNET.Exceptions;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;
using System;
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
        ///   Sends a custom SPL token. Handles the associated token account
        /// </summary>
        /// <param name="rpcClient"> </param>
        /// <param name="receiver"> </param>
        /// <param name="sender"> </param>
        /// <param name="tokenMint"> </param>
        /// <param name="amount"> The amount in Sol to send </param>
        /// <returns> </returns>
        public static async Task<OneOf<bool, SolmangoRpcException>> SendSplToken(IRpcClient rpcClient, Account sender, string receiver, string tokenMint, double amount)
        {
            // Get the blockhash
            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            if (blockHash is null) return false;
            if (!blockHash.WasRequestSuccessfullyHandled) return new SolmangoRpcException(blockHash.Reason, blockHash.ServerErrorCode);

            // Get the sender ata
            var fromAta = await GetAssociatedTokenAccount(rpcClient, sender.PublicKey, tokenMint);
            if (fromAta is null) return false;

            // Get the token decimals and the actual amount
            var res = await rpcClient.GetTokenSupplyAsync(tokenMint);
            if (res is null) return false;
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

            // Perform transaction
            var result = await rpcClient.SendTransactionAsync(Convert.ToBase64String(transaction));
            return result is null
                ? (OneOf<bool, SolmangoRpcException>)false
                : !result.WasRequestSuccessfullyHandled ? new SolmangoRpcException(result.Reason, result.ServerErrorCode) : true;
        }
    }
}
using Solnet.Rpc;
using Solnet.Rpc.Types;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame
{
    public class SolanaEndpointManager : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private Cluster cluster;
        [SerializeField] private bool useCustomEndpoint = false;
        [SerializeField] private string customEndpoint = null;
        [SerializeField] private bool connectOnInit = true;
        [SerializeField] private BatchAutoExecuteMode batchAutoExecuteMode = BatchAutoExecuteMode.ExecuteWithCallbackFailures;
        [SerializeField] private int triggerCount = 1;
        private SolanaRpcBatchWithCallbacks rpcBatcher;

        public static SolanaEndpointManager Instance { get; private set; } = null;

        public IStreamingRpcClient StreamingRpcClient { get; private set; }

        public IRpcClient RpcClient { get; private set; }

        /// <summary>
        ///   Add a request to the <see cref="SolanaRpcBatchWithCallbacks"/>
        /// </summary>
        /// <param name="requestFactory"> </param>
        public void BatchRequest(Action<SolanaRpcBatchWithCallbacks> requestFactory) => requestFactory.Invoke(rpcBatcher);

        /// <summary>
        ///   Flush the <see cref="SolanaRpcBatchWithCallbacks"/> non blocking
        /// </summary>
        public void FlushBatch() => _ = Task.Run(() => rpcBatcher.Flush());

        private async void OnDestroy()
        {
            await StreamingRpcClient?.DisconnectAsync();
            StreamingRpcClient?.Dispose();
        }

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            await Initialize();
        }

        private async Task Initialize()
        {
            StreamingRpcClient = useCustomEndpoint ? ClientFactory.GetStreamingClient(customEndpoint) : ClientFactory.GetStreamingClient(cluster);
            RpcClient = useCustomEndpoint ? ClientFactory.GetClient(customEndpoint) : ClientFactory.GetClient(cluster);
            rpcBatcher = new SolanaRpcBatchWithCallbacks(RpcClient);
            rpcBatcher.AutoExecute(batchAutoExecuteMode, triggerCount);
            if (connectOnInit)
            {
                await StreamingRpcClient.ConnectAsync();
            }
        }
    }
}
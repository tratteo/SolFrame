using SolFrame.Utils;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame
{
    // Using custom editor
    public class SolanaEndpointManager : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private Cluster cluster = Cluster.DevNet;
        [SerializeField] private bool useCustomEndpoint = false;
        [SerializeField] private string customEndpoint = null;
        [SerializeField] private string customStreamingEndpoint = null;
        [SerializeField] private bool connectOnInit = true;
        [SerializeField] private BatchAutoExecuteMode batchAutoExecuteMode = BatchAutoExecuteMode.ExecuteWithCallbackFailures;
        [SerializeField] private int triggerCount = 1;
        [SerializeField] private bool enableLogs = true;
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
        ///   Flush the <see cref="SolanaRpcBatchWithCallbacks"/>
        /// </summary>
        public async Task FlushBatchAsync() => await Task.Run(() => rpcBatcher.Flush());

        /// <summary>
        ///   Connect the <see cref="IStreamingRpcClient"/>
        /// </summary>
        /// <returns> </returns>
        public async Task ConnectStreamingAsync()
        {
            await StreamingRpcClient.ConnectAsync();
            this.LogObj("IStreamingRpcClient connected", ref enableLogs);
        }

        /// <summary>
        ///   Disconnect the <see cref="IStreamingRpcClient"/>
        /// </summary>
        /// <returns> </returns>
        public async Task DisconnectStreamingAsync()
        {
            await StreamingRpcClient.DisconnectAsync();
            this.LogObj("IStreamingRpcClient disconnected", ref enableLogs);
        }

        private void OnDestroy() => _ = DisconnectStreamingAsync();

        private void Awake()
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
            _ = Initialize();
        }

        private async Task Initialize()
        {
            var endpoint = useCustomEndpoint ? customStreamingEndpoint : Enum.GetName(typeof(Cluster), cluster);
            StreamingRpcClient = useCustomEndpoint ? ClientFactory.GetStreamingClient(customStreamingEndpoint) : ClientFactory.GetStreamingClient(cluster);
            this.LogObj($"Initialized IStreamingRpcClient [{endpoint}]", ref enableLogs);

            endpoint = useCustomEndpoint ? customEndpoint : Enum.GetName(typeof(Cluster), cluster);
            RpcClient = useCustomEndpoint ? ClientFactory.GetClient(customEndpoint) : ClientFactory.GetClient(cluster);
            this.LogObj($"Initialized IRpcClient [{endpoint}]", ref enableLogs);

            if (connectOnInit) await ConnectStreamingAsync();

            rpcBatcher = new SolanaRpcBatchWithCallbacks(RpcClient);
            rpcBatcher.AutoExecute(batchAutoExecuteMode, triggerCount);
        }
    }
}
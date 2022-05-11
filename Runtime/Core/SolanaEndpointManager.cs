using Solnet.Rpc;
using UnityEngine;

namespace SolFrame
{
    public class SolanaEndpointManager : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private Cluster cluster;
        [SerializeField] private bool useCustomEndpoint = false;
        [SerializeField] private string customEndpoint = null;
        private IRpcClient rpcClient;

        public SolanaEndpointManager Instance { get; private set; } = null;

        public IRpcClient GetClient()
        {
            rpcClient ??= useCustomEndpoint ? ClientFactory.GetClient(customEndpoint) : ClientFactory.GetClient(cluster);
            return rpcClient;
        }

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
        }
    }
}
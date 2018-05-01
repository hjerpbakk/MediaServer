namespace SlackIntegration.Configuration {
	public class AppConfig : ISlackConfig, IBlogStorageConfig {
        public string BlobStorageConnectionString { get; set; }
        public string SlackToken { get; set; }
        public bool UseTestSlackChannel { get; set; }
    }
}

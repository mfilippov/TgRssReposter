# Telegram RSS Reposter

The tool repost messages from RSS feed to Telegram channel.

Configuration via environment variable:
* **FEED_URL** - RSS feed URL
* **STORAGE_FILE** - path to DB file for store published post state
* **API_TOKEN** - Telegram Bot API token
* **CHANNEL_ID** - Telegram channel id like: *@test123*
* **RHASH** - Instant view rhash ID if you like to have instant view for post links
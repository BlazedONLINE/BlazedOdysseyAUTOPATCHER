# BlazedOdyssey API Server Deployment Guide

## Prerequisites
- Digital Ocean Droplet (Ubuntu 22.04) - IP: 129.212.181.87
- MySQL already installed and running
- SSH access to your droplet

## Step 1: Prepare Your Local Files

First, let's create a deployment package with all necessary files:

```bash
# Create a deployment folder
mkdir blazed-api-deploy
cd blazed-api-deploy

# Copy server files (run this from your project root)
cp -r "BlazedOdysseyMMO/BlazedOdysseyMMOAlpha/Database/server/" ./
cp "BlazedOdysseyMMO/BlazedOdysseyMMOAlpha/Database/create_accounts_table.sql" ./
cp "BlazedOdysseyMMO/BlazedOdysseyMMOAlpha/Database/create_characters_table.sql" ./
```

## Step 2: Upload Files to Your Droplet

```bash
# Upload the entire deployment package
scp -r ./blazed-api-deploy/ gm@129.212.181.87:~/

# Or if you prefer individual files:
scp -r ./server/ gm@129.212.181.87:~/blazed-api/
scp *.sql gm@129.212.181.87:~/
```

## Step 3: SSH into Your Droplet

```bash
ssh gm@129.212.181.87
```

## Step 4: Install Node.js (if not already installed)

```bash
# Update package list
sudo apt update

# Install Node.js 18.x (LTS)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Verify installation
node --version
npm --version
```

## Step 5: Set Up MySQL Database

```bash
# Start MySQL service
sudo systemctl start mysql
sudo systemctl enable mysql

# Create database and import tables
mysql -u gm -p blazed_odyssey_db < ~/create_accounts_table.sql
mysql -u gm -p blazed_odyssey_db < ~/create_characters_table.sql

# Verify tables were created
mysql -u gm -p -e "USE blazed_odyssey_db; SHOW TABLES;"
```

## Step 6: Set Up the API Server

```bash
# Navigate to server directory
cd ~/blazed-api-deploy/server/

# Install dependencies
npm install

# Create production environment file
cat > .env << EOF
PORT=3000
NODE_ENV=production
DB_HOST=localhost
DB_PORT=3306
DB_USER=gm
DB_PASSWORD=.bLanch33x
DB_NAME=blazed_odyssey_db
EOF

# Test the server
node server.js
```

## Step 7: Install PM2 for Process Management

```bash
# Install PM2 globally
sudo npm install -g pm2

# Start the API server with PM2
pm2 start server.js --name "blazed-api"

# Save PM2 configuration
pm2 save

# Set PM2 to start on boot
pm2 startup
# Follow the command it gives you (usually involves sudo)

# Check status
pm2 status
pm2 logs blazed-api
```

## Step 8: Configure Firewall

```bash
# Allow necessary ports
sudo ufw allow 22    # SSH
sudo ufw allow 3000  # API Server
sudo ufw allow 3306  # MySQL (only if needed externally)

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status
```

## Step 9: Test the API

```bash
# Test locally on the droplet
curl http://localhost:3000/api/health

# Test from your local machine
curl http://129.212.181.87:3000/api/health
```

## Step 10: Optional - Set Up Nginx Reverse Proxy

If you want to use port 80 instead of 3000:

```bash
# Install Nginx
sudo apt install nginx

# Create Nginx configuration
sudo tee /etc/nginx/sites-available/blazed-api << EOF
server {
    listen 80;
    server_name 129.212.181.87;

    location /api/ {
        proxy_pass http://localhost:3000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Enable the site
sudo ln -s /etc/nginx/sites-available/blazed-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx

# Update firewall
sudo ufw allow 80
```

## Troubleshooting

### Check if services are running:
```bash
# Check PM2 status
pm2 status

# Check MySQL status
sudo systemctl status mysql

# Check Nginx status (if using)
sudo systemctl status nginx

# Check application logs
pm2 logs blazed-api

# Check system logs
journalctl -u mysql
```

### Common Issues:

1. **Port 3000 blocked**: Make sure UFW allows port 3000
2. **MySQL connection refused**: Ensure MySQL is running and credentials are correct
3. **Permission denied**: Check file permissions in ~/blazed-api-deploy/
4. **Module not found**: Run `npm install` in the server directory

### Test Endpoints:

```bash
# Health check
curl http://129.212.181.87:3000/api/health

# Check username
curl http://129.212.181.87:3000/api/accounts/check-username/testuser

# Check character name
curl http://129.212.181.87:3000/api/characters/check-name/TestCharacter
```

## Unity Configuration

Once the API server is running, update your Unity `UnityWebDatabase.cs`:

```csharp
[SerializeField] private string apiBaseUrl = "http://129.212.181.87:3000/api";
```

## Security Notes

- Change the default database password
- Consider using environment variables for sensitive data
- Set up SSL/TLS for production use
- Regularly update your system and Node.js packages
- Monitor server logs for suspicious activity
#!/bin/bash

# BlazedOdyssey API Server Auto-Deployment Script
# Run this script on your Digital Ocean droplet

set -e  # Exit on any error

echo "🚀 BlazedOdyssey API Server Deployment Starting..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
   print_error "This script should not be run as root"
   exit 1
fi

# Update system packages
print_status "Updating system packages..."
sudo apt update && sudo apt upgrade -y

# Install Node.js if not present
if ! command -v node &> /dev/null; then
    print_status "Installing Node.js..."
    curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
    sudo apt-get install -y nodejs
else
    print_status "Node.js already installed: $(node --version)"
fi

# Install PM2 if not present
if ! command -v pm2 &> /dev/null; then
    print_status "Installing PM2..."
    sudo npm install -g pm2
else
    print_status "PM2 already installed"
fi

# Check if MySQL is running
if ! sudo systemctl is-active --quiet mysql; then
    print_status "Starting MySQL service..."
    sudo systemctl start mysql
    sudo systemctl enable mysql
else
    print_status "MySQL service is already running"
fi

# Set up project directory
PROJECT_DIR="$HOME/blazed-api"
if [ -d "$PROJECT_DIR" ]; then
    print_warning "Project directory exists. Backing up..."
    mv "$PROJECT_DIR" "$PROJECT_DIR.backup.$(date +%Y%m%d_%H%M%S)"
fi

mkdir -p "$PROJECT_DIR"
cd "$PROJECT_DIR"

# Install dependencies
if [ -f "package.json" ]; then
    print_status "Installing Node.js dependencies..."
    npm install
else
    print_error "package.json not found. Please ensure server files are uploaded."
    exit 1
fi

# Create environment file
print_status "Creating environment configuration..."
cat > .env << EOF
PORT=3000
NODE_ENV=production
DB_HOST=localhost
DB_PORT=3306
DB_USER=gm
DB_PASSWORD=.bLanch33x
DB_NAME=blazed_odyssey_db
EOF

# Import database tables if SQL files exist
if [ -f "$HOME/create_accounts_table.sql" ]; then
    print_status "Importing accounts table..."
    mysql -u gm -p".bLanch33x" blazed_odyssey_db < "$HOME/create_accounts_table.sql" || print_warning "Accounts table import failed (may already exist)"
fi

if [ -f "$HOME/create_characters_table.sql" ]; then
    print_status "Importing characters table..."
    mysql -u gm -p".bLanch33x" blazed_odyssey_db < "$HOME/create_characters_table.sql" || print_warning "Characters table import failed (may already exist)"
fi

# Test database connection
print_status "Testing database connection..."
if mysql -u gm -p".bLanch33x" -e "USE blazed_odyssey_db; SELECT 1;" &> /dev/null; then
    print_status "Database connection successful"
else
    print_error "Database connection failed"
    exit 1
fi

# Stop existing PM2 process if running
if pm2 list | grep -q "blazed-api"; then
    print_status "Stopping existing API server..."
    pm2 stop blazed-api
    pm2 delete blazed-api
fi

# Start the API server with PM2
print_status "Starting API server with PM2..."
pm2 start server.js --name "blazed-api"

# Save PM2 configuration
pm2 save

# Set up PM2 to start on boot
print_status "Configuring PM2 startup..."
PM2_STARTUP_CMD=$(pm2 startup | tail -1)
if [[ $PM2_STARTUP_CMD == sudo* ]]; then
    print_warning "Please run the following command manually:"
    echo "$PM2_STARTUP_CMD"
fi

# Configure firewall
print_status "Configuring firewall..."
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 3000/tcp  # API Server
sudo ufw --force enable

# Test the API server
print_status "Testing API server..."
sleep 3  # Give the server time to start

if curl -s http://localhost:3000/api/health > /dev/null; then
    print_status "API server is running successfully!"
else
    print_error "API server test failed"
    pm2 logs blazed-api --lines 10
    exit 1
fi

# Display status
print_status "Deployment completed successfully!"
echo ""
echo "🎉 BlazedOdyssey API Server is now running!"
echo ""
echo "📊 Server Status:"
pm2 status
echo ""
echo "🌐 API Endpoints:"
echo "  Health Check: http://129.212.181.87:3000/api/health"
echo "  Accounts: http://129.212.181.87:3000/api/accounts/"
echo "  Characters: http://129.212.181.87:3000/api/characters/"
echo ""
echo "📋 Useful Commands:"
echo "  View logs: pm2 logs blazed-api"
echo "  Restart API: pm2 restart blazed-api"
echo "  Stop API: pm2 stop blazed-api"
echo "  Server status: pm2 status"
echo ""
print_status "Your Unity client can now connect to: http://129.212.181.87:3000/api"
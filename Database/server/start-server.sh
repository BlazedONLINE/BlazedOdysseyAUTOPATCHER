#!/bin/bash

# BlazedOdyssey API Server Startup Script
# Run this on your Digital Ocean droplet

echo "🚀 Starting BlazedOdyssey API Server..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Installing Node.js..."
    curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
    sudo apt-get install -y nodejs
fi

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "❌ npm is not installed. Installing npm..."
    sudo apt-get install -y npm
fi

echo "📦 Installing dependencies..."
npm install

echo "🗄️ Checking MySQL connection..."
# Test if MySQL is running
if ! sudo systemctl is-active --quiet mysql; then
    echo "🔄 Starting MySQL service..."
    sudo systemctl start mysql
    sudo systemctl enable mysql
fi

echo "✅ Starting API server..."
npm start
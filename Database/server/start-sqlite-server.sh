#!/bin/bash

echo "🔄 Starting BlazedOdyssey SQLite API Server..."

# Kill any existing server process
pkill -f "node.*server"

# Navigate to server directory
cd "$(dirname "$0")"

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm install
fi

# Start the SQLite server
echo "🚀 Starting SQLite server..."
npm run start-sqlite
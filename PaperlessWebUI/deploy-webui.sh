#!/bin/bash

# Paperless WebUI Docker Deployment Script
echo "🚀 Starting Paperless WebUI Docker Deployment..."

# Set variables
IMAGE_NAME="paperless-webui"
CONTAINER_NAME="paperless-webui-container"
PORT="3000"

# Function to cleanup existing container
cleanup() {
    echo "🧹 Cleaning up existing containers and images..."
    
    # Stop and remove container if exists
    if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
        echo "Stopping existing container: $CONTAINER_NAME"
        docker stop $CONTAINER_NAME
        docker rm $CONTAINER_NAME
    fi
    
    # Remove image if exists
    if [ "$(docker images -q $IMAGE_NAME)" ]; then
        echo "Removing existing image: $IMAGE_NAME"
        docker rmi $IMAGE_NAME
    fi
}

# Function to build Docker image
build_image() {
    echo "🔨 Building Docker image..."
    
    # Check if Dockerfile exists
    if [ ! -f "Dockerfile" ]; then
        echo "❌ Error: Dockerfile not found in current directory"
        exit 1
    fi
    
    # Build the image
    docker build -t $IMAGE_NAME . --no-cache
    
    if [ $? -eq 0 ]; then
        echo "✅ Docker image built successfully"
    else
        echo "❌ Failed to build Docker image"
        exit 1
    fi
}

# Function to run container
run_container() {
    echo "🏃 Running Docker container..."
    
    docker run -d \
        --name $CONTAINER_NAME \
        -p $PORT:80 \
        --restart unless-stopped \
        $IMAGE_NAME
    
    if [ $? -eq 0 ]; then
        echo "✅ Container started successfully"
        echo "🌐 WebUI is available at: http://localhost:$PORT"
    else
        echo "❌ Failed to start container"
        exit 1
    fi
}

# Function to show container logs
show_logs() {
    echo "📋 Container logs:"
    docker logs $CONTAINER_NAME --tail 20
}

# Function to show container status
show_status() {
    echo "📊 Container status:"
    docker ps -f name=$CONTAINER_NAME
    
    echo ""
    echo "📈 Container stats:"
    docker stats $CONTAINER_NAME --no-stream
}

# Function to test nginx
test_nginx() {
    echo "🧪 Testing Nginx configuration..."
    
    # Wait a moment for container to start
    sleep 3
    
    # Test if nginx is responding
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$PORT)
    
    if [ "$HTTP_STATUS" = "200" ]; then
        echo "✅ Nginx is responding correctly (HTTP $HTTP_STATUS)"
        
        # Test API proxy
        echo "🔗 Testing API proxy..."
        API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$PORT/api/health)
        
        if [ "$API_STATUS" = "502" ] || [ "$API_STATUS" = "503" ]; then
            echo "⚠️  API proxy configured but backend not available (HTTP $API_STATUS) - This is expected"
        else
            echo "📡 API proxy status: HTTP $API_STATUS"
        fi
        
    else
        echo "❌ Nginx not responding correctly (HTTP $HTTP_STATUS)"
        echo "📋 Checking container logs:"
        docker logs $CONTAINER_NAME
    fi
}

# Function to open browser
open_browser() {
    echo "🌐 Opening browser..."
    
    # Detect OS and open browser
    case "$OSTYPE" in
        darwin*)  open http://localhost:$PORT ;;
        linux*)   xdg-open http://localhost:$PORT ;;
        msys*)    start http://localhost:$PORT ;;
        *)        echo "Please open http://localhost:$PORT in your browser" ;;
    esac
}

# Function to show help
show_help() {
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  build     Build Docker image only"
    echo "  run       Run container only (requires existing image)"
    echo "  deploy    Full deployment (cleanup + build + run)"
    echo "  test      Test nginx and API proxy"
    echo "  logs      Show container logs"
    echo "  status    Show container status"
    echo "  stop      Stop and remove container"
    echo "  restart   Restart container"
    echo "  open      Open browser"
    echo "  help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 deploy    # Full deployment"
    echo "  $0 test      # Test after deployment"
    echo "  $0 logs      # View logs"
}

# Main execution
case "${1:-deploy}" in
    "build")
        build_image
        ;;
    "run")
        run_container
        ;;
    "deploy")
        cleanup
        build_image
        run_container
        echo ""
        echo "⏳ Waiting for container to start..."
        sleep 5
        test_nginx
        echo ""
        echo "🎉 Deployment completed!"
        echo "📝 Next steps:"
        echo "   - View logs: $0 logs"
        echo "   - Check status: $0 status"
        echo "   - Open browser: $0 open"
        ;;
    "test")
        test_nginx
        ;;
    "logs")
        show_logs
        ;;
    "status")
        show_status
        ;;
    "stop")
        echo "🛑 Stopping container..."
        docker stop $CONTAINER_NAME
        docker rm $CONTAINER_NAME
        echo "✅ Container stopped and removed"
        ;;
    "restart")
        echo "🔄 Restarting container..."
        docker restart $CONTAINER_NAME
        sleep 3
        test_nginx
        ;;
    "open")
        open_browser
        ;;
    "help"|"--help"|"-h")
        show_help
        ;;
    *)
        echo "❌ Unknown command: $1"
        show_help
        exit 1
        ;;
esac

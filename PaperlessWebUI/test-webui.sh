#!/bin/bash

# WebUI Testing Script
# Tests the WebUI in Docker container

set -e

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

CONTAINER_NAME="paperless-webui-container"
BASE_URL="http://localhost:3000"

print_test() {
    echo -e "${BLUE}🧪 Testing: $1${NC}"
}

print_pass() {
    echo -e "${GREEN}✅ PASS: $1${NC}"
}

print_fail() {
    echo -e "${RED}❌ FAIL: $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# Test if container is running
test_container_running() {
    print_test "Container Status"
    if docker ps -q -f name=${CONTAINER_NAME} | grep -q .; then
        print_pass "Container is running"
        return 0
    else
        print_fail "Container is not running"
        return 1
    fi
}

# Test HTTP response
test_http_response() {
    print_test "HTTP Response"
    
    local status_code=$(curl -s -o /dev/null -w "%{http_code}" ${BASE_URL})
    
    if [ "$status_code" = "200" ]; then
        print_pass "HTTP 200 OK"
        return 0
    else
        print_fail "HTTP Status: $status_code"
        return 1
    fi
}

# Test page content
test_page_content() {
    print_test "Page Content"
    
    local content=$(curl -s ${BASE_URL})
    
    if echo "$content" | grep -q "Paperless DMS"; then
        print_pass "Page contains expected content"
        return 0
    else
        print_fail "Page does not contain expected content"
        return 1
    fi
}

# Test JavaScript loading
test_js_loading() {
    print_test "JavaScript Assets"
    
    local content=$(curl -s ${BASE_URL})
    
    if echo "$content" | grep -q "script.*src"; then
        print_pass "JavaScript assets found"
        return 0
    else
        print_fail "No JavaScript assets found"
        return 1
    fi
}

# Test API proxy (if backend is running)
test_api_proxy() {
    print_test "API Proxy"
    
    local status_code=$(curl -s -o /dev/null -w "%{http_code}" ${BASE_URL}/api/health 2>/dev/null || echo "000")
    
    case "$status_code" in
        "200")
            print_pass "API proxy working (backend connected)"
            ;;
        "502"|"503"|"504")
            print_info "API proxy configured but backend not available"
            ;;
        "404")
            print_info "API endpoint not found (expected without backend)"
            ;;
        *)
            print_info "API proxy status: $status_code"
            ;;
    esac
}

# Performance test
test_performance() {
    print_test "Performance"
    
    local response_time=$(curl -s -o /dev/null -w "%{time_total}" ${BASE_URL})
    local response_time_ms=$(echo "$response_time * 1000" | bc)
    
    if (( $(echo "$response_time < 2" | bc -l) )); then
        print_pass "Response time: ${response_time_ms%.*}ms (Good)"
    elif (( $(echo "$response_time < 5" | bc -l) )); then
        print_info "Response time: ${response_time_ms%.*}ms (Acceptable)"
    else
        print_fail "Response time: ${response_time_ms%.*}ms (Slow)"
    fi
}

# Memory usage test
test_memory_usage() {
    print_test "Memory Usage"
    
    local memory_usage=$(docker stats --no-stream --format "{{.MemUsage}}" ${CONTAINER_NAME} 2>/dev/null || echo "N/A")
    
    if [ "$memory_usage" != "N/A" ]; then
        print_info "Memory usage: $memory_usage"
    else
        print_info "Memory usage: Not available"
    fi
}

# Run all tests
run_tests() {
    echo -e "${BLUE}🚀 Starting WebUI Tests${NC}\n"
    
    local failed_tests=0
    local total_tests=0
    
    # Wait a moment for container to be ready
    sleep 2
    
    # Basic tests
    tests=(
        "test_container_running"
        "test_http_response"
        "test_page_content"
        "test_js_loading"
    )
    
    for test in "${tests[@]}"; do
        total_tests=$((total_tests + 1))
        if ! $test; then
            failed_tests=$((failed_tests + 1))
        fi
        echo
    done
    
    # Additional tests (non-critical)
    test_api_proxy
    echo
    test_performance
    echo
    test_memory_usage
    echo
    
    # Summary
    local passed_tests=$((total_tests - failed_tests))
    echo -e "${BLUE}📊 Test Summary${NC}"
    echo "Total Tests: $total_tests"
    echo "Passed: $passed_tests"
    echo "Failed: $failed_tests"
    
    if [ $failed_tests -eq 0 ]; then
        echo -e "\n${GREEN}🎉 All tests passed!${NC}"
        return 0
    else
        echo -e "\n${RED}❌ $failed_tests test(s) failed${NC}"
        return 1
    fi
}

# Show container logs on failure
show_logs() {
    echo -e "\n${YELLOW}📋 Container Logs:${NC}"
    docker logs --tail 20 ${CONTAINER_NAME} 2>/dev/null || echo "No logs available"
}

# Main execution
main() {
    if run_tests; then
        echo -e "\n${GREEN}✨ WebUI is working correctly!${NC}"
        echo -e "${BLUE}🌐 Access at: ${BASE_URL}${NC}"
    else
        show_logs
        echo -e "\n${RED}🔧 Please check the issues above${NC}"
        exit 1
    fi
}

# Check dependencies
check_dependencies() {
    if ! command -v curl &> /dev/null; then
        print_fail "curl is required but not installed"
        exit 1
    fi
    
    if ! command -v bc &> /dev/null; then
        print_info "bc not found, performance test will be skipped"
    fi
}

check_dependencies
main

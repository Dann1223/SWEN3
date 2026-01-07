import apiClient from './api';
import type { 
  AdvancedSearchRequest, 
  AdvancedSearchResponse,
  SearchResult
} from '../types/search.types';

export const searchService = {
  // Advanced search with Elasticsearch
  async search(request: AdvancedSearchRequest): Promise<AdvancedSearchResponse> {
    return apiClient.post('/api/search/documents', request);
  },

  // Get search suggestions
  async getSuggestions(query: string, maxSuggestions = 10): Promise<string[]> {
    const params = new URLSearchParams();
    if (query) params.append('query', query);
    if (maxSuggestions) params.append('maxSuggestions', maxSuggestions.toString());
    
    return apiClient.get(`/api/search/suggestions?${params.toString()}`);
  },

  // Get search aggregations
  async getAggregations(query?: string): Promise<Record<string, any>> {
    const params = new URLSearchParams();
    if (query) params.append('query', query);
    
    return apiClient.get(`/api/search/aggregations?${params.toString()}`);
  },

  // Get similar documents
  async getSimilarDocuments(documentId: number, maxResults = 10): Promise<SearchResult[]> {
    return apiClient.get(`/api/search/similar/${documentId}?maxResults=${maxResults}`);
  },

  // Check search service health
  async checkHealth(): Promise<any> {
    return apiClient.get('/api/search/health');
  },

  // Create search index
  async createIndex(): Promise<any> {
    return apiClient.post('/api/search/index/create');
  },

  // Delete search index
  async deleteIndex(): Promise<any> {
    return apiClient.delete('/api/search/index');
  },

  // Rebuild search index
  async rebuildIndex(): Promise<any> {
    return apiClient.post('/api/search/index/rebuild');
  },

  // Sync document indexing status
  async syncIndexingStatus(): Promise<any> {
    return apiClient.post('/api/search/sync-status');
  }
};

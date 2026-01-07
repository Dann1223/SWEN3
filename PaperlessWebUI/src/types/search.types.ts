export interface SearchFilters {
  query?: string;
  tagIds?: number[];
  fileType?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: 'uploadDate' | 'title' | 'fileSize';
  sortOrder?: 'asc' | 'desc';
}

export interface SearchRequest extends SearchFilters {
  page?: number;
  pageSize?: number;
}

export interface SearchSuggestion {
  text: string;
  type: 'title' | 'content' | 'tag';
  count: number;
}

// Advanced search types based on backend API
export interface AdvancedSearchRequest {
  query?: string;
  tags?: string[];
  fileTypes?: string[];
  dateFrom?: string;
  dateTo?: string;
  processedOnly?: boolean;
  aiProcessedOnly?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
  enableHighlight?: boolean;
  minConfidence?: number;
}

export interface SearchResult {
  id: number;
  title: string;
  fileName: string;
  fileType: string;
  contentSnippet?: string;
  summary?: string;
  tags: string[];
  uploadDate: string;
  lastModified?: string;
  fileSize: number;
  score: number;
  confidence?: number;
  hasAccess: boolean;
  permissionLevel?: string;
  highlights?: Record<string, string[]>;
}

export interface AdvancedSearchResponse {
  results: SearchResult[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  executionTime: number;
  hasMore: boolean;
  aggregations?: Record<string, any>;
}

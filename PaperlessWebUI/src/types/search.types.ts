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

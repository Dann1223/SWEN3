import { createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { SearchFilters } from '../../types/search.types';

interface SearchState {
  filters: SearchFilters;
  isSearching: boolean;
  lastSearchQuery: string;
  searchHistory: string[];
  suggestions: string[];
}

const initialState: SearchState = {
  filters: {
    query: '',
    tagIds: [],
    sortBy: 'uploadDate',
    sortOrder: 'desc',
  },
  isSearching: false,
  lastSearchQuery: '',
  searchHistory: [],
  suggestions: [],
};

const searchSlice = createSlice({
  name: 'search',
  initialState,
  reducers: {
    setSearchFilters: (state, action: PayloadAction<SearchFilters>) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    setSearchQuery: (state, action: PayloadAction<string>) => {
      state.filters.query = action.payload;
    },
    setIsSearching: (state, action: PayloadAction<boolean>) => {
      state.isSearching = action.payload;
    },
    addToSearchHistory: (state, action: PayloadAction<string>) => {
      const query = action.payload.trim();
      if (query && !state.searchHistory.includes(query)) {
        state.searchHistory = [query, ...state.searchHistory.slice(0, 9)]; // Keep last 10
      }
      state.lastSearchQuery = query;
    },
    clearSearchHistory: (state) => {
      state.searchHistory = [];
    },
    setSuggestions: (state, action: PayloadAction<string[]>) => {
      state.suggestions = action.payload;
    },
    clearSearch: (state) => {
      state.filters = {
        query: '',
        tagIds: [],
        sortBy: 'uploadDate',
        sortOrder: 'desc',
      };
      state.isSearching = false;
    },
  },
});

export const {
  setSearchFilters,
  setSearchQuery,
  setIsSearching,
  addToSearchHistory,
  clearSearchHistory,
  setSuggestions,
  clearSearch,
} = searchSlice.actions;

export default searchSlice.reducer;

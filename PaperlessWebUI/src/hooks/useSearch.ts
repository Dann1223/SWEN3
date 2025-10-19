import { useSelector, useDispatch } from 'react-redux';
import { useCallback, useMemo } from 'react';
import type { RootState, AppDispatch } from '../store';
import {
  setSearchFilters,
  setSearchQuery,
  setIsSearching,
  addToSearchHistory,
  clearSearch,
} from '../store/slices/searchSlice';
import { searchDocuments } from '../store/slices/documentSlice';
import type { SearchFilters } from '../types/search.types';

export const useSearch = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { filters, isSearching, lastSearchQuery, searchHistory } = useSelector(
    (state: RootState) => state.search
  );
  const { documents, loading } = useSelector((state: RootState) => state.documents);

  const updateFilters = useCallback(
    (newFilters: Partial<SearchFilters>) => {
      dispatch(setSearchFilters(newFilters));
    },
    [dispatch]
  );

  const updateQuery = useCallback(
    (query: string) => {
      dispatch(setSearchQuery(query));
    },
    [dispatch]
  );

  const performSearch = useCallback(
    async (searchFilters?: SearchFilters) => {
      const searchParams = searchFilters || filters;
      
      if (!searchParams.query?.trim()) {
        return;
      }

      dispatch(setIsSearching(true));
      
      try {
        await dispatch(searchDocuments({
          ...searchParams,
          page: 1,
          pageSize: 20,
        }));
        
        // Add to search history
        if (searchParams.query) {
          dispatch(addToSearchHistory(searchParams.query));
        }
      } catch (error) {
        console.error('Search failed:', error);
      } finally {
        dispatch(setIsSearching(false));
      }
    },
    [dispatch, filters]
  );

  const clearSearchState = useCallback(() => {
    dispatch(clearSearch());
  }, [dispatch]);

  const searchResults = useMemo(() => {
    return isSearching || filters.query ? documents : [];
  }, [documents, isSearching, filters.query]);

  const hasActiveSearch = useMemo(() => {
    return !!(filters.query || filters.tagIds?.length || filters.fileType);
  }, [filters]);

  return {
    filters,
    isSearching,
    loading,
    lastSearchQuery,
    searchHistory,
    searchResults,
    hasActiveSearch,
    updateFilters,
    updateQuery,
    performSearch,
    clearSearchState,
  };
};

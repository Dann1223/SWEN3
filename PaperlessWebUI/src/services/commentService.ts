import apiClient from './api';
import type { 
  Comment, 
  CreateCommentRequest, 
  UpdateCommentRequest,
  CommentStatistics
} from '../types/comment.types';

export const commentService = {
  // Get comments for a document
  async getByDocumentId(documentId: number, includeReplies = true): Promise<Comment[]> {
    return apiClient.get(`/api/comments/document/${documentId}?includeReplies=${includeReplies}`);
  },

  // Get comment by ID
  async getById(commentId: number, includeReplies = true): Promise<Comment> {
    return apiClient.get(`/api/comments/${commentId}?includeReplies=${includeReplies}`);
  },

  // Create new comment
  async create(comment: CreateCommentRequest): Promise<Comment> {
    return apiClient.post('/api/comments', comment);
  },

  // Update comment
  async update(commentId: number, data: UpdateCommentRequest): Promise<Comment> {
    return apiClient.put(`/api/comments/${commentId}`, data);
  },

  // Delete comment
  async delete(commentId: number): Promise<void> {
    return apiClient.delete(`/api/comments/${commentId}`);
  },

  // Get replies for a comment
  async getReplies(commentId: number): Promise<Comment[]> {
    return apiClient.get(`/api/comments/${commentId}/replies`);
  },

  // Get recent comments
  async getRecent(limit = 20): Promise<Comment[]> {
    return apiClient.get(`/api/comments/recent?limit=${limit}`);
  },

  // Get comment statistics for a document
  async getStatistics(documentId: number): Promise<CommentStatistics> {
    return apiClient.get(`/api/comments/document/${documentId}/statistics`);
  }
};

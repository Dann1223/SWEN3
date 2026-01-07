export interface Comment {
  id: number;
  documentId: number;
  authorName: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
  parentCommentId?: number;
  position?: string;
  isEdited: boolean;
  replyCount: number;
  replies?: Comment[];
}

export interface CreateCommentRequest {
  documentId: number;
  authorName: string;
  content: string;
  parentCommentId?: number;
  position?: string;
}

export interface UpdateCommentRequest {
  content: string;
}

export interface CommentStatistics {
  totalComments: number;
  totalReplies: number;
  activeCommenters: number;
  lastCommentDate?: string;
}

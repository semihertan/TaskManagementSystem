import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  Input,
  OnInit,
  inject
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { CommentService } from '../../../core/services/comment.service';
import { TaskComment } from '../../interfaces/comment/task-comment.interface';

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './comment-section.html',
  styleUrl: './comment-section.scss'
})
export class CommentSection implements OnInit {
  private commentService = inject(CommentService);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) taskId!: string;

  comments: TaskComment[] = [];

  newCommentContent = '';
  editingCommentId: string | null = null;
  editingContent = '';

  isLoading = false;
  isCreating = false;
  updatingCommentId: string | null = null;
  deletingCommentId: string | null = null;

  ngOnInit(): void {
    this.loadComments();
  }

  loadComments(): void {
    if (!this.taskId) {
      return;
    }

    this.isLoading = true;

    this.commentService
      .getComments(this.taskId)
      .subscribe({
        next: (comments) => {
          this.comments = comments ?? [];
          this.isLoading = false;
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Yorumlar alınamadı:', error);

          this.comments = [];
          this.isLoading = false;

          this.snackBar.open(
            'Yorumlar yüklenemedi.',
            'Kapat',
            {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  createComment(): void {
    const content = this.newCommentContent.trim();

    if (!content || this.isCreating) {
      return;
    }

    this.isCreating = true;

    this.commentService
      .createComment(this.taskId, { content })
      .subscribe({
        next: () => {
          this.newCommentContent = '';
          this.isCreating = false;

          this.loadComments();

          this.snackBar.open(
            'Yorum başarıyla eklendi.',
            'Kapat',
            {
              duration: 2500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );
        },

        error: (error) => {
          console.error('Yorum eklenemedi:', error);

          this.isCreating = false;

          this.snackBar.open(
            error?.error?.message ??
              'Yorum eklenirken bir hata oluştu.',
            'Kapat',
            {
              duration: 3500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  startEditing(comment: TaskComment): void {
    this.editingCommentId = comment.id;
    this.editingContent = comment.content;
  }

  cancelEditing(): void {
    this.editingCommentId = null;
    this.editingContent = '';
  }

  updateComment(commentId: string): void {
    const content = this.editingContent.trim();

    if (!content || this.updatingCommentId) {
      return;
    }

    this.updatingCommentId = commentId;

    this.commentService
      .updateComment(
        this.taskId,
        commentId,
        { content }
      )
      .subscribe({
        next: () => {
          this.updatingCommentId = null;
          this.cancelEditing();

          this.loadComments();

          this.snackBar.open(
            'Yorum güncellendi.',
            'Kapat',
            {
              duration: 2500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );
        },

        error: (error) => {
          console.error('Yorum güncellenemedi:', error);

          this.updatingCommentId = null;

          this.snackBar.open(
            error?.error?.message ??
              'Yorum güncellenirken bir hata oluştu.',
            'Kapat',
            {
              duration: 3500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  deleteComment(commentId: string): void {
    const confirmed = window.confirm(
      'Bu yorumu silmek istediğinize emin misiniz?'
    );

    if (!confirmed || this.deletingCommentId) {
      return;
    }

    this.deletingCommentId = commentId;

    this.commentService
      .deleteComment(this.taskId, commentId)
      .subscribe({
        next: () => {
          this.deletingCommentId = null;

          this.comments = this.comments.filter(
            comment => comment.id !== commentId
          );

          this.snackBar.open(
            'Yorum silindi.',
            'Kapat',
            {
              duration: 2500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Yorum silinemedi:', error);

          this.deletingCommentId = null;

          this.snackBar.open(
            error?.error?.message ??
              'Yorum silinirken bir hata oluştu.',
            'Kapat',
            {
              duration: 3500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  onCommentKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.createComment();
    }
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
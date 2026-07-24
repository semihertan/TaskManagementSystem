import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';

import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';

import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../shared/interfaces/category/category.interface';
import { CategoryForm } from './category-form/category-form';


@Component({
  selector: 'app-categories',
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {
  private categoryService = inject(CategoryService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private cdr = inject(ChangeDetectorRef);

  categories: Category[] = [];

  isLoading = false;
  errorMessage = '';
  deletingCategoryId: string | null = null;

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.categoryService
      .getCategories()
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (response) => {
          this.categories = response.data;
        },

        error: (error) => {
          console.error('Kategoriler alınamadı:', error);

          this.errorMessage =
            error?.error?.message ??
            'Kategoriler yüklenirken bir hata oluştu.';
        }
      });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CategoryForm, {
      width: '520px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((createdCategory: Category | undefined) => {
      if (!createdCategory) {
        return;
      }

      this.categories = [
        ...this.categories,
        createdCategory
      ];

      this.snackBar.open(
        'Kategori başarıyla oluşturuldu.',
        'Kapat',
        {
          duration: 3000
        }
      );

      this.cdr.markForCheck();
    });
  }

  openEditDialog(category: Category): void {
    const dialogRef = this.dialog.open(CategoryForm, {
      width: '520px',
      maxWidth: '95vw',
      data: category
    });

    dialogRef.afterClosed().subscribe((updatedCategory: Category | undefined) => {
      if (!updatedCategory) {
        return;
      }

      this.categories = this.categories.map((item) =>
        item.id === updatedCategory.id
          ? updatedCategory
          : item
      );

      this.snackBar.open(
        'Kategori başarıyla güncellendi.',
        'Kapat',
        {
          duration: 3000
        }
      );

      this.cdr.markForCheck();
    });
  }

  deleteCategory(category: Category): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      width: '420px',
      maxWidth: '95vw',
      data: {
        title: 'Kategori silinsin mi?',
        message: `"${category.name}" kategorisini silmek istediğinize emin misiniz?`,
        confirmText: 'Sil',
        cancelText: 'İptal'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) {
        return;
      }

      this.categoryService
          .deleteCategory(category.id)
            .pipe(
      finalize(() => {
        this.deletingCategoryId = null;
        this.cdr.markForCheck();
      })
    )
    .subscribe({
      next: () => {
        this.categories = this.categories.filter(
          (item) => item.id !== category.id
        );

            this.cdr.markForCheck();
          },

          error: (error) => {
            console.error('Kategori silinemedi:', error);

            this.snackBar.open(
              error?.error?.message ??
                'Kategori silinirken bir hata oluştu.',
              'Kapat',
              {
                duration: 4000
              }
            );
          }
        });
    });
  }
}
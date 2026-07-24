import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { finalize } from 'rxjs';

import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../shared/interfaces/category/category.interface';
import { CreateCategory } from '../../../shared/interfaces/category/create-category.interface';
import { UpdateCategory } from '../../../shared/interfaces/category/update-category.interface';

@Component({
  selector: 'app-category-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,

    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,

    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './category-form.html',
  styleUrl: './category-form.scss'
})
export class CategoryForm {
  private formBuilder = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private dialogRef = inject(MatDialogRef<CategoryForm>);

  category = inject<Category | null>(MAT_DIALOG_DATA, {
    optional: true
  });

  isEditMode = !!this.category;

  isSaving = false;
  errorMessage = '';

  categoryForm = this.formBuilder.group({
    name: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],

    description: [
      '',
      [
        Validators.maxLength(500)
      ]
    ],

    color: [
      '#2196F3',
      [
        Validators.required,
        Validators.pattern(/^#[0-9A-Fa-f]{6}$/)
      ]
    ]
  });

  constructor() {
    if (!this.category) {
      return;
    }

    this.categoryForm.patchValue({
      name: this.category.name,
      description: this.category.description ?? '',
      color: this.category.color
    });
  }

  save(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formValue = this.categoryForm.getRawValue();

    const request = {
      name: formValue.name!,
      description: formValue.description || undefined,
      color: formValue.color!
    };

    if (this.isEditMode && this.category) {
      this.updateCategory(request);
      return;
    }

    this.createCategory(request);
  }

  private createCategory(request: CreateCategory): void {
    this.categoryService
      .createCategory(request)
      .pipe(
        finalize(() => {
          this.isSaving = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.dialogRef.close(response.data);
        },

        error: (error) => {
          console.error('Kategori oluşturulamadı:', error);

          this.errorMessage =
            error?.error?.message ??
            'Kategori oluşturulurken bir hata oluştu.';
        }
      });
  }

  private updateCategory(request: UpdateCategory): void {
    if (!this.category) {
      return;
    }

    this.categoryService
      .updateCategory(
        this.category.id,
        request
      )
      .pipe(
        finalize(() => {
          this.isSaving = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.dialogRef.close(response.data);
        },

        error: (error) => {
          console.error('Kategori güncellenemedi:', error);

          this.errorMessage =
            error?.error?.message ??
            'Kategori güncellenirken bir hata oluştu.';
        }
      });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
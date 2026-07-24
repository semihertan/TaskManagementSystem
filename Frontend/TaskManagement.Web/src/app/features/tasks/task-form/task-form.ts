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
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { provideNativeDateAdapter } from '@angular/material/core';

import { TaskService } from '../../../core/services/task.service';
import { TaskItem } from '../../../shared/interfaces/task/task.interface';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../shared/interfaces/category/category.interface';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-task-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,

    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,

    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatProgressSpinnerModule
  ],
  providers:[
    provideNativeDateAdapter()
  ],
  templateUrl: './task-form.html',
  styleUrl: './task-form.scss',
})
export class TaskForm {
  private formBuilder = inject(FormBuilder);
  private taskService = inject(TaskService);
  private dialogRef = inject(MatDialogRef<TaskForm>);
  private categoryService = inject(CategoryService);

  categories: Category[] = [];
  isCategoriesLoading = false;

  task = inject<TaskItem | null>(MAT_DIALOG_DATA, {
    optional: true
  });

  isEditMode = !!this.task;
  
  isSaving = false;
  errorMessage = '';

  taskForm = this.formBuilder.group({
    title: ['', [
      Validators.required,
      Validators.maxLength(200)
    ]],

    description: ['', Validators.maxLength(2000)],

    priority: [3,[ 
      Validators.required,
      Validators.min(1),
      Validators.max(5)
    ]],

    status: [0, [ 
      Validators.required,
      Validators.min(0),
      Validators.max(3)
    ]],

    dueDate: [null as Date | null],
    categoryId: [null as string | null]
  });

  statuses = [
    { value: 0, label: 'Bekliyor' },
    { value: 1, label: 'Devam Ediyor' },
    { value: 2, label: 'Tamamlandı' },
    { value: 3, label: 'İptal Edildi' }
  ];

  constructor() {
    this.loadCategories();

    if (!this.task) {
      return;
    }

    this.taskForm.patchValue({
      title: this.task.title,
      description: this.task.description ?? '',
      priority: this.task.priority,
      status: this.task.status,
      dueDate: this.task.dueDate
        ? new Date(this.task.dueDate)
        : null
    });
  }

  save(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formValue = this.taskForm.getRawValue();

    const request = {
      title: formValue.title!,
      description: formValue.description || undefined,
      priority: formValue.priority!,
      status: formValue.status!,
      categoryId: formValue.categoryId || undefined,
      dueDate: formValue.dueDate
        ? formValue.dueDate.toISOString()
        : undefined
    };

    if (this.isEditMode && this.task) {
      this.updateTask(request);
      return;
    }

    this.createTask(request);
  }

  private createTask(request: any): void {
    this.taskService
      .createTask(request)
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
        console.error('Görev oluşturulamadı:', error);

        this.errorMessage =
          error?.error?.message ??
          'Görev oluşturulurken bir hata oluştu.';
      }
    });
  }

  private updateTask(request: any): void {
    if (!this.task) {
      return;
    }

    this.taskService
      .updateTask(this.task.id, request)
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
          console.error('Görev güncellenemedi:', error);

          this.errorMessage =
            error?.error?.message ??
            'Görev güncellenirken bir hata oluştu.';
        }
      });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private loadCategories(): void {
    this.isCategoriesLoading = true;

    this.taskForm.controls.categoryId.disable();

    this.categoryService
      .getCategories()
      .pipe(
        finalize(() => {
          this.isCategoriesLoading = false;
          this.taskForm.controls.categoryId.enable();
        })
      )
      .subscribe({
        next: (response) => {
          this.categories = response.data;
        },

        error: (error) => {
          console.error('Kategoriler yüklenemedi:', error);

          this.errorMessage =
            error?.error?.message ??
            'Kategoriler yüklenirken bir hata oluştu.';
        }
      });
  }
}
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
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

  isSaving = false;
  errorMessage = '';

  taskForm = this.formBuilder.group({
    title: ['', [
      Validators.required,
      Validators.maxLength(200)
    ]],

    description: [''],

    priority: [3, Validators.required],

    dueDate: [null as Date | null]
  });

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
      dueDate: formValue.dueDate
        ? formValue.dueDate.toISOString()
        : undefined
    };

    this.taskService.createTask(request).subscribe({
      next: (response) => {
        this.dialogRef.close(response.data);
      },

      error: (error) => {
        console.error('Görev oluşturulamadı:', error);

        this.errorMessage =
          error?.error?.message ??
          'Görev oluşturulurken bir hata oluştu.';

        this.isSaving = false;
      }
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
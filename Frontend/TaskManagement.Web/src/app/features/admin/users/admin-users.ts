import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin.service';
import { User } from '../../../shared/interfaces/auth/user.interface';
import { UserRole } from '../../../shared/interfaces/auth/user-role.enum';

@Component({
  selector: 'app-admin-users',
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSnackBarModule
  ],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminUsers implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly UserRole = UserRole;
  readonly roleOptions = [
    { value: UserRole.User, label: 'User' },
    { value: UserRole.Admin, label: 'Admin' }
  ];

  users: User[] = [];
  isLoading = false;
  errorMessage = '';
  updatingUserId: string | null = null;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.adminService
      .getUsers()
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: response => {
          this.users = response.data ?? [];
        },
        error: error => {
          this.errorMessage = error?.error?.message ??
            'Kullanıcılar yüklenirken bir hata oluştu.';
        }
      });
  }

  updateRole(user: User, role: UserRole): void {
    if (user.role === role || this.updatingUserId) {
      return;
    }

    this.updatingUserId = user.id;

    this.adminService
      .updateRole(user.id, role)
      .pipe(finalize(() => {
        this.updatingUserId = null;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: response => {
          this.replaceUser(response.data);
          this.snackBar.open(
            'Kullanıcı rolü güncellendi. Yeni rol bir sonraki girişte geçerli olur.',
            'Kapat',
            { duration: 4000 }
          );
        },
        error: error => {
          this.showError(error?.error?.message ?? 'Kullanıcı rolü güncellenemedi.');
        }
      });
  }

  updateStatus(user: User, isActive: boolean): void {
    if (user.isActive === isActive || this.updatingUserId) {
      return;
    }

    this.updatingUserId = user.id;

    this.adminService
      .updateStatus(user.id, isActive)
      .pipe(finalize(() => {
        this.updatingUserId = null;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: response => {
          this.replaceUser(response.data);
          this.snackBar.open(
            isActive ? 'Kullanıcı aktifleştirildi.' : 'Kullanıcı pasifleştirildi.',
            'Kapat',
            { duration: 3000 }
          );
        },
        error: error => {
          this.showError(error?.error?.message ?? 'Kullanıcı durumu güncellenemedi.');
        }
      });
  }

  private replaceUser(updatedUser: User | null): void {
    if (!updatedUser) {
      return;
    }

    this.users = this.users.map(user =>
      user.id === updatedUser.id ? updatedUser : user
    );
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: 4000,
      panelClass: ['error-snackbar']
    });
  }
}

import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { Reservation, ReservationStatus } from '../../../core/models';
import { getReservationStatusLabel } from '../../../core/labels/labels';

@Component({
  selector: 'app-admin-reservations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-reservations.component.html',
  styleUrl: './admin-reservations.component.scss'
})
export class AdminReservationsComponent {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  reservation?: Reservation;
  message = '';
  errorMessage = '';

  form = this.fb.group({
    reservationId: ['', Validators.required]
  });

  loadReservation(): void {
    const id = this.form.value.reservationId?.trim();
    if (!id) return;

    this.message = '';
    this.errorMessage = '';
    this.api.getReservation(id).subscribe({
      next: reservation => (this.reservation = reservation),
      error: () => {
        this.reservation = undefined;
        this.errorMessage = 'Reserva no encontrada.';
      }
    });
  }

  confirmPayment(): void {
    if (!this.reservation) return;

    this.api.confirmPayment(this.reservation.id).subscribe({
      next: reservation => {
        this.reservation = reservation;
        this.message = `Pago confirmado. Código: ${reservation.reservationCode}`;
      },
      error: err => (this.errorMessage = err.error?.Message ?? err.error?.message ?? 'No se pudo confirmar el pago.')
    });
  }

  cancelReservation(): void {
    if (!this.reservation) return;

    this.api.cancelReservation(this.reservation.id).subscribe({
      next: reservation => {
        this.reservation = reservation;
        this.message = `Reserva cancelada. Estado: ${getReservationStatusLabel(reservation.status)}`;
      },
      error: err => (this.errorMessage = err.error?.Message ?? err.error?.message ?? 'No se pudo cancelar la reserva.')
    });
  }

  getStatusLabel(status: ReservationStatus): string {
    return getReservationStatusLabel(status);
  }
}

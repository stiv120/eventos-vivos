import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { Event } from '../../../core/models';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.scss'
})
export class ReservationFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  event?: Event;
  eventId = '';
  successMessage = '';
  errorMessage = '';
  isSubmitting = false;

  form = this.fb.group({
    quantity: [1, [Validators.required, Validators.min(1)]],
    buyerName: ['', Validators.required],
    buyerEmail: ['', [Validators.required, Validators.email]]
  });

  ngOnInit(): void {
    this.eventId = this.route.snapshot.paramMap.get('id') ?? '';
    this.api.getEvent(this.eventId).subscribe({
      next: event => (this.event = event),
      error: () => (this.errorMessage = 'Evento no encontrado.')
    });
  }

  submit(): void {
    if (this.form.invalid || !this.eventId) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.createReservation({
      eventId: this.eventId,
      quantity: value.quantity!,
      buyerName: value.buyerName!,
      buyerEmail: value.buyerEmail!
    }).subscribe({
      next: reservation => {
        this.successMessage = `Reserva creada (ID: ${reservation.id}). Estado: Pendiente de pago.`;
        this.isSubmitting = false;
      },
      error: err => {
        this.errorMessage = err.error?.Message ?? err.error?.message ?? 'No se pudo crear la reserva.';
        this.isSubmitting = false;
      }
    });
  }
}

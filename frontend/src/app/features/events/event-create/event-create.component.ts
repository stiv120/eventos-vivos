import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { EventType, Venue } from '../../../core/models';
import { EVENT_TYPE_LABELS } from '../../../core/labels/labels';

@Component({
  selector: 'app-event-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-create.component.html',
  styleUrl: './event-create.component.scss'
})
export class EventCreateComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  venues: Venue[] = [];
  eventTypes = [
    { value: EventType.Conference, label: EVENT_TYPE_LABELS[EventType.Conference] },
    { value: EventType.Workshop, label: EVENT_TYPE_LABELS[EventType.Workshop] },
    { value: EventType.Concert, label: EVENT_TYPE_LABELS[EventType.Concert] }
  ];

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    venueId: [null as number | null, Validators.required],
    maxCapacity: [null as number | null, [Validators.required, Validators.min(1)]],
    startDateTime: ['', Validators.required],
    endDateTime: ['', Validators.required],
    ticketPrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    type: [EventType.Conference, Validators.required]
  });

  successMessage = '';
  errorMessage = '';
  isSubmitting = false;

  ngOnInit(): void {
    this.api.getVenues().subscribe({
      next: venues => (this.venues = venues),
      error: () => (this.errorMessage = 'No se pudieron cargar los venues.')
    });
  }

  get selectedVenue(): Venue | undefined {
    const venueId = this.form.value.venueId;
    return this.venues.find(v => v.id === venueId);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api.createEvent({
      title: value.title!,
      description: value.description!,
      venueId: value.venueId!,
      maxCapacity: value.maxCapacity!,
      startDateTime: new Date(value.startDateTime!).toISOString(),
      endDateTime: new Date(value.endDateTime!).toISOString(),
      ticketPrice: value.ticketPrice!,
      type: value.type!
    }).subscribe({
      next: () => {
        this.successMessage = 'Evento creado correctamente.';
        this.form.reset({ type: EventType.Conference });
        this.isSubmitting = false;
      },
      error: err => {
        this.errorMessage = err.error?.Message ?? err.error?.message ?? 'No se pudo crear el evento.';
        this.isSubmitting = false;
      }
    });
  }
}

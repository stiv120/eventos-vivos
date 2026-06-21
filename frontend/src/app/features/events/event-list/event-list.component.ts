import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { Event, EventFilter, EventStatus, EventType, Venue } from '../../../core/models';
import {
  EVENT_STATUS_LABELS,
  EVENT_TYPE_LABELS,
  getEventStatusLabel,
  getEventTypeLabel
} from '../../../core/labels/labels';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss'
})
export class EventListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);

  events: Event[] = [];
  venues: Venue[] = [];
  errorMessage = '';

  filterForm = this.fb.group({
    type: [''],
    status: [''],
    venueId: [''],
    titleSearch: [''],
    startDateFrom: [''],
    startDateTo: ['']
  });

  eventTypes = [
    { value: EventType.Conference, label: EVENT_TYPE_LABELS[EventType.Conference] },
    { value: EventType.Workshop, label: EVENT_TYPE_LABELS[EventType.Workshop] },
    { value: EventType.Concert, label: EVENT_TYPE_LABELS[EventType.Concert] }
  ];

  eventStatuses = [
    { value: EventStatus.Active, label: EVENT_STATUS_LABELS[EventStatus.Active] },
    { value: EventStatus.Cancelled, label: EVENT_STATUS_LABELS[EventStatus.Cancelled] },
    { value: EventStatus.Completed, label: EVENT_STATUS_LABELS[EventStatus.Completed] }
  ];

  ngOnInit(): void {
    this.api.getVenues().subscribe(venues => (this.venues = venues));
    this.loadEvents();
  }

  loadEvents(): void {
    const raw = this.filterForm.getRawValue();
    const filter: EventFilter = {};

    if (raw.type) filter.type = Number(raw.type) as EventType;
    if (raw.status) filter.status = Number(raw.status) as EventStatus;
    if (raw.venueId) filter.venueId = Number(raw.venueId);
    if (raw.titleSearch) filter.titleSearch = raw.titleSearch;
    if (raw.startDateFrom) filter.startDateFrom = new Date(raw.startDateFrom).toISOString();
    if (raw.startDateTo) filter.startDateTo = new Date(raw.startDateTo).toISOString();

    this.api.getEvents(filter).subscribe({
      next: events => {
        this.events = events;
        this.errorMessage = '';
      },
      error: () => (this.errorMessage = 'No se pudieron cargar los eventos.')
    });
  }

  getTypeLabel(type: EventType): string {
    return getEventTypeLabel(type);
  }

  getStatusLabel(status: EventStatus): string {
    return getEventStatusLabel(status);
  }
}

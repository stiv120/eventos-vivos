import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateEventRequest,
  CreateReservationRequest,
  Event,
  EventFilter,
  OccupancyReport,
  Reservation,
  Venue
} from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getVenues(): Observable<Venue[]> {
    return this.http.get<Venue[]>(`${this.baseUrl}/venues`);
  }

  getEvents(filter: EventFilter = {}): Observable<Event[]> {
    let params = new HttpParams();

    if (filter.type !== undefined) params = params.set('type', filter.type);
    if (filter.startDateFrom) params = params.set('startDateFrom', filter.startDateFrom);
    if (filter.startDateTo) params = params.set('startDateTo', filter.startDateTo);
    if (filter.venueId !== undefined) params = params.set('venueId', filter.venueId);
    if (filter.status !== undefined) params = params.set('status', filter.status);
    if (filter.titleSearch) params = params.set('titleSearch', filter.titleSearch);

    return this.http.get<Event[]>(`${this.baseUrl}/events`, { params });
  }

  getEvent(id: string): Observable<Event> {
    return this.http.get<Event>(`${this.baseUrl}/events/${id}`);
  }

  createEvent(request: CreateEventRequest): Observable<Event> {
    return this.http.post<Event>(`${this.baseUrl}/events`, request);
  }

  getOccupancyReport(eventId: string): Observable<OccupancyReport> {
    return this.http.get<OccupancyReport>(`${this.baseUrl}/events/${eventId}/occupancy-report`);
  }

  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.baseUrl}/reservations`, request);
  }

  getReservation(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.baseUrl}/reservations/${id}`);
  }

  confirmPayment(reservationId: string): Observable<Reservation> {
    const headers: Record<string, string> = {};
    if (environment.adminApiKey) {
      headers['X-Admin-Key'] = environment.adminApiKey;
    }

    return this.http.post<Reservation>(
      `${this.baseUrl}/reservations/${reservationId}/confirm-payment`,
      {},
      { headers }
    );
  }

  cancelReservation(reservationId: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.baseUrl}/reservations/${reservationId}/cancel`, {});
  }
}

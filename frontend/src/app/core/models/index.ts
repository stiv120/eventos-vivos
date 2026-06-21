export enum EventType {
  Conference = 1,
  Workshop = 2,
  Concert = 3
}

export enum EventStatus {
  Active = 1,
  Cancelled = 2,
  Completed = 3
}

export enum ReservationStatus {
  PendingPayment = 1,
  Confirmed = 2,
  Cancelled = 3,
  Lost = 4
}

export interface Venue {
  id: number;
  name: string;
  capacity: number;
  city: string;
}

export interface Event {
  id: string;
  title: string;
  description: string;
  venueId: number;
  venueName: string;
  venueCity: string;
  maxCapacity: number;
  startDateTime: string;
  endDateTime: string;
  ticketPrice: number;
  type: EventType;
  status: EventStatus;
  availableSeats: number;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  venueId: number;
  maxCapacity: number;
  startDateTime: string;
  endDateTime: string;
  ticketPrice: number;
  type: EventType;
}

export interface EventFilter {
  type?: EventType;
  startDateFrom?: string;
  startDateTo?: string;
  venueId?: number;
  status?: EventStatus;
  titleSearch?: string;
}

export interface Reservation {
  id: string;
  eventId: string;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
  status: ReservationStatus;
  reservationCode: string | null;
  createdAt: string;
  cancelledAt: string | null;
}

export interface CreateReservationRequest {
  eventId: string;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
}

export interface OccupancyReport {
  eventId: string;
  eventTitle: string;
  totalSoldTickets: number;
  availableTickets: number;
  occupancyPercentage: number;
  totalRevenue: number;
  status: EventStatus;
}

export interface ApiError {
  code: string;
  message: string;
  errors?: Record<string, string[]>;
}

import { EventStatus, EventType, ReservationStatus } from '../models';

export const EVENT_TYPE_LABELS: Record<EventType, string> = {
  [EventType.Conference]: 'Conferencia',
  [EventType.Workshop]: 'Taller',
  [EventType.Concert]: 'Concierto'
};

export const EVENT_STATUS_LABELS: Record<EventStatus, string> = {
  [EventStatus.Active]: 'Activo',
  [EventStatus.Cancelled]: 'Cancelado',
  [EventStatus.Completed]: 'Completado'
};

export const RESERVATION_STATUS_LABELS: Record<ReservationStatus, string> = {
  [ReservationStatus.PendingPayment]: 'Pendiente de pago',
  [ReservationStatus.Confirmed]: 'Confirmada',
  [ReservationStatus.Cancelled]: 'Cancelada',
  [ReservationStatus.Lost]: 'Perdida'
};

export function getEventTypeLabel(type: EventType): string {
  return EVENT_TYPE_LABELS[type] ?? 'Desconocido';
}

export function getEventStatusLabel(status: EventStatus): string {
  return EVENT_STATUS_LABELS[status] ?? 'Desconocido';
}

export function getReservationStatusLabel(status: ReservationStatus): string {
  return RESERVATION_STATUS_LABELS[status] ?? 'Desconocido';
}

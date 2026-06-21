import { Routes } from '@angular/router';
import { EventListComponent } from './features/events/event-list/event-list.component';
import { EventCreateComponent } from './features/events/event-create/event-create.component';
import { ReservationFormComponent } from './features/reservations/reservation-form/reservation-form.component';
import { AdminReservationsComponent } from './features/admin/admin-reservations/admin-reservations.component';
import { OccupancyReportComponent } from './features/events/occupancy-report/occupancy-report.component';

export const routes: Routes = [
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  { path: 'events', component: EventListComponent },
  { path: 'events/create', component: EventCreateComponent },
  { path: 'events/:id/reserve', component: ReservationFormComponent },
  { path: 'events/:id/report', component: OccupancyReportComponent },
  { path: 'admin/reservations', component: AdminReservationsComponent }
];

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { EventStatus, OccupancyReport } from '../../../core/models';
import { getEventStatusLabel } from '../../../core/labels/labels';

@Component({
  selector: 'app-occupancy-report',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './occupancy-report.component.html',
  styleUrl: './occupancy-report.component.scss'
})
export class OccupancyReportComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);

  report?: OccupancyReport;
  errorMessage = '';

  ngOnInit(): void {
    const eventId = this.route.snapshot.paramMap.get('id') ?? '';
    this.api.getOccupancyReport(eventId).subscribe({
      next: report => (this.report = report),
      error: () => (this.errorMessage = 'No se pudo cargar el reporte de ocupación.')
    });
  }

  getStatusLabel(status: EventStatus): string {
    return getEventStatusLabel(status);
  }
}

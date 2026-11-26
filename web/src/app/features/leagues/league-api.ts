import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateLeagueRequest, League } from './league';

@Injectable({ providedIn: 'root' })
export class LeagueApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/v1/leagues`;

  getLeagues(): Observable<League[]> {
    return this.http.get<League[]>(this.baseUrl);
  }

  createLeague(request: CreateLeagueRequest): Observable<League> {
    return this.http.post<League>(this.baseUrl, request);
  }
}
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { CreateLeagueRequest, League } from './league';
import { LeagueApi } from './league-api';

describe('LeagueApi', () => {
  let api: LeagueApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    api = TestBed.inject(LeagueApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('gets the list of leagues from the API', () => {
    // Arrange
    const leagues: League[] = [
      { id: 1, name: 'Adams League' },
      { id: 2, name: 'Bay League' },
    ];
    let result: League[] | undefined;

    // Act
    api.getLeagues().subscribe((response) => (result = response));

    const request = httpTesting.expectOne(`${environment.apiBaseUrl}/api/v1/leagues`);
    request.flush(leagues);

    // Assert
    expect(request.request.method).toBe('GET');
    expect(result).toEqual(leagues);
  });

  it('posts a new league to the API and returns the created league', () => {
    // Arrange
    const newLeague: CreateLeagueRequest = { name: 'Cedar League' };
    const createdLeague: League = { id: 3, name: 'Cedar League' };
    let result: League | undefined;

    // Act
    api.createLeague(newLeague).subscribe((response) => (result = response));

    const request = httpTesting.expectOne(`${environment.apiBaseUrl}/api/v1/leagues`);
    request.flush(createdLeague, { status: 201, statusText: 'Created' });

    // Assert
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(newLeague);
    expect(result).toEqual(createdLeague);
  });
});

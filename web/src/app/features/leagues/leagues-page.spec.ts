import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { League } from './league';
import { LeagueApi } from './league-api';
import { LeaguesPage } from './leagues-page';

describe('LeaguesPage', () => {
  const leagues: League[] = [
    { id: 2, name: 'adams league' },
    { id: 3, name: 'Bay League' },
    { id: 1, name: 'Cedar League' },
  ];

  beforeEach(async () => {
    const fakeLeagueApi: Partial<LeagueApi> = {
      getLeagues: () => of(leagues),
    };

    await TestBed.configureTestingModule({
      imports: [LeaguesPage],
      providers: [{ provide: LeagueApi, useValue: fakeLeagueApi }],
    }).compileComponents();
  });

  it('shows every league name in the order the API sends them', async () => {
    // Arrange
    const fixture = TestBed.createComponent(LeaguesPage);

    // Act
    await fixture.whenStable();

    // Assert
    const page: HTMLElement = fixture.nativeElement;
    const names = Array.from(page.querySelectorAll('li')).map((item) => item.textContent?.trim());

    expect(names).toEqual(['adams league', 'Bay League', 'Cedar League']);
  });
});
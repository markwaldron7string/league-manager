import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { League } from './league';
import { LeagueApi } from './league-api';
import { LeaguesPage } from './leagues-page';

describe('LeaguesPage', () => {
  async function renderPage(leagues: League[]): Promise<HTMLElement> {
    const fakeLeagueApi: Partial<LeagueApi> = {
      getLeagues: () => of(leagues),
    };

    TestBed.configureTestingModule({
      imports: [LeaguesPage],
      providers: [{ provide: LeagueApi, useValue: fakeLeagueApi }],
    });

    const fixture = TestBed.createComponent(LeaguesPage);
    await fixture.whenStable();

    return fixture.nativeElement;
  }

  it('shows every league name in the order the API sends them', async () => {
    // Arrange
    const leagues: League[] = [
      { id: 2, name: 'adams league' },
      { id: 3, name: 'Bay League' },
      { id: 1, name: 'Cedar League' },
    ];

    // Act
    const page = await renderPage(leagues);

    // Assert
    const names = Array.from(page.querySelectorAll('li')).map((item) => item.textContent?.trim());
    expect(names).toEqual(['adams league', 'Bay League', 'Cedar League']);
  });

  it('shows "No leagues yet." when there are no leagues', async () => {
    // Act
    const page = await renderPage([]);

    // Assert
    expect(page.textContent).toContain('No leagues yet.');
    expect(page.querySelector('ul')).toBeNull();
  });

    it('has a form with a name field and a Create button', async () => {
    // Act
    const page = await renderPage([]);

    // Assert
    const label = Array.from(page.querySelectorAll('label')).find(
      (item) => item.textContent?.trim() === 'League name',
    );
    expect(label).toBeDefined();

    const nameField = page.querySelector(`#${label?.htmlFor}`);
    expect(nameField?.tagName).toBe('INPUT');
    expect(nameField?.closest('form')).not.toBeNull();

    const createButton = Array.from(page.querySelectorAll('button')).find(
      (item) => item.textContent?.trim() === 'Create',
    );
    expect(createButton?.type).toBe('submit');
  });
});
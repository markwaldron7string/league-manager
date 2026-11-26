import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { League } from './league';
import { LeagueApi } from './league-api';
import { LeaguesPage } from './leagues-page';

describe('LeaguesPage', () => {
  async function renderPage(
    fakeLeagueApi: Partial<LeagueApi>,
  ): Promise<ComponentFixture<LeaguesPage>> {
    TestBed.configureTestingModule({
      imports: [LeaguesPage],
      providers: [{ provide: LeagueApi, useValue: fakeLeagueApi }],
    });

    const fixture = TestBed.createComponent(LeaguesPage);
    await fixture.whenStable();

    return fixture;
  }

  function findButton(page: HTMLElement, text: string): HTMLButtonElement {
    const button = Array.from(page.querySelectorAll('button')).find(
      (item) => item.textContent?.trim() === text,
    );

    if (!button) {
      throw new Error(`No button with the text "${text}" was found.`);
    }

    return button;
  }

  it('shows every league name in the order the API sends them', async () => {
    // Arrange
    const leagues: League[] = [
      { id: 2, name: 'adams league' },
      { id: 3, name: 'Bay League' },
      { id: 1, name: 'Cedar League' },
    ];

    // Act
    const fixture = await renderPage({ getLeagues: () => of(leagues) });

    // Assert
    const page: HTMLElement = fixture.nativeElement;
    const names = Array.from(page.querySelectorAll('li')).map((item) => item.textContent?.trim());
    expect(names).toEqual(['adams league', 'Bay League', 'Cedar League']);
  });

  it('shows "No leagues yet." when there are no leagues', async () => {
    // Act
    const fixture = await renderPage({ getLeagues: () => of([]) });

    // Assert
    const page: HTMLElement = fixture.nativeElement;
    expect(page.textContent).toContain('No leagues yet.');
    expect(page.querySelector('ul')).toBeNull();
  });

  it('has a form with a name field and a Create button', async () => {
    // Act
    const fixture = await renderPage({ getLeagues: () => of([]) });

    // Assert
    const page: HTMLElement = fixture.nativeElement;
    const label = Array.from(page.querySelectorAll('label')).find(
      (item) => item.textContent?.trim() === 'League name',
    );
    expect(label).toBeDefined();

    const nameField = page.querySelector(`#${label?.htmlFor}`);
    expect(nameField?.tagName).toBe('INPUT');
    expect(nameField?.closest('form')).not.toBeNull();

    expect(findButton(page, 'Create').type).toBe('submit');
  });

  it('adds a new league to the list without reloading the page', async () => {
    // Arrange
    const createdLeague: League = { id: 1, name: 'Cedar League' };
    const getLeagues = vi
      .fn()
      .mockReturnValueOnce(of([]))
      .mockReturnValueOnce(of([createdLeague]));
    const createLeague = vi.fn().mockReturnValue(of(createdLeague));

    const fixture = await renderPage({ getLeagues, createLeague });
    const page: HTMLElement = fixture.nativeElement;

    const nameField = page.querySelector<HTMLInputElement>('#league-name');
    if (!nameField) {
      throw new Error('The name field was not found.');
    }

    let submitEvent: Event | undefined;
    nameField.form?.addEventListener('submit', (event) => (submitEvent = event));

    // Act
    nameField.value = 'Cedar League';
    nameField.dispatchEvent(new Event('input'));
    findButton(page, 'Create').click();
    await fixture.whenStable();

    // Assert
    expect(createLeague).toHaveBeenCalledWith({ name: 'Cedar League' });
    expect(submitEvent?.defaultPrevented).toBe(true);

    const names = Array.from(page.querySelectorAll('li')).map((item) => item.textContent?.trim());
    expect(names).toEqual(['Cedar League']);
    expect(nameField.value).toBe('');
  });

    it.each([
    { name: '', message: 'League name is required.' },
    { name: '   ', message: 'League name is required.' },
    { name: 'a'.repeat(101), message: 'League name must be 100 characters or fewer.' },
  ])('shows "$message" and sends no request for an invalid name', async ({ name, message }) => {
    // Arrange
    const createLeague = vi.fn();
    const fixture = await renderPage({ getLeagues: () => of([]), createLeague });
    const page: HTMLElement = fixture.nativeElement;

    const nameField = page.querySelector<HTMLInputElement>('#league-name');
    if (!nameField) {
      throw new Error('The name field was not found.');
    }

    // Act
    nameField.value = name;
    nameField.dispatchEvent(new Event('input'));
    findButton(page, 'Create').click();
    await fixture.whenStable();

    // Assert
    expect(page.textContent).toContain(message);
    expect(createLeague).not.toHaveBeenCalled();
  });
});
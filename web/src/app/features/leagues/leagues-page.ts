import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';

import { HttpErrorResponse } from '@angular/common/http';
import { CreateLeagueRequest, League } from './league';
import { LeagueApi } from './league-api';

function notBlank(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { blank: true } : null;
}

function problemDetail(error: HttpErrorResponse): string | undefined {
  const detail: unknown = error.error?.detail;
  return typeof detail === 'string' ? detail : undefined;
}

@Component({
  selector: 'app-leagues-page',
  imports: [ReactiveFormsModule],
  templateUrl: './leagues-page.html',
  styleUrl: './leagues-page.scss',
})
export class LeaguesPage implements OnInit {
  private readonly leagueApi = inject(LeagueApi);

  protected readonly leagues = signal<League[] | undefined>(undefined);

  protected readonly createError = signal<string | undefined>(undefined);

  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [notBlank, Validators.maxLength(100)],
    }),
  });

  ngOnInit(): void {
    this.loadLeagues();
  }

  protected createLeague(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: CreateLeagueRequest = this.form.getRawValue();
    this.createError.set(undefined);

    this.leagueApi.createLeague(request).subscribe({
      next: () => {
        this.form.reset();
        this.loadLeagues();
      },
      error: (error: HttpErrorResponse) => {
        this.createError.set(problemDetail(error) ?? 'The league could not be created.');
      },
    });
  }

  private loadLeagues(): void {
    this.leagueApi.getLeagues().subscribe({
      next: (leagues) => {
        this.loadError.set(undefined);
        this.leagues.set(leagues);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(problemDetail(error) ?? 'The leagues could not be loaded.');
      },
    });
  }
}

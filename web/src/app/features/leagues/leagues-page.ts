import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';

import { CreateLeagueRequest, League } from './league';
import { LeagueApi } from './league-api';

function notBlank(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { blank: true } : null;
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

    this.leagueApi.createLeague(request).subscribe(() => {
      this.form.reset();
      this.loadLeagues();
    });
  }

  private loadLeagues(): void {
    this.leagueApi.getLeagues().subscribe((leagues) => this.leagues.set(leagues));
  }
}

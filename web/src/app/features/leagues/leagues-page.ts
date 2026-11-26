import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { League } from './league';
import { LeagueApi } from './league-api';

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
    name: new FormControl('', { nonNullable: true }),
  });

  ngOnInit(): void {
    this.loadLeagues();
  }

  private loadLeagues(): void {
    this.leagueApi.getLeagues().subscribe((leagues) => this.leagues.set(leagues));
  }
}
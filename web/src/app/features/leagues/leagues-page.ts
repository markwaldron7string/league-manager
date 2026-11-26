import { Component, OnInit, inject, signal } from '@angular/core';

import { League } from './league';
import { LeagueApi } from './league-api';

@Component({
  selector: 'app-leagues-page',
  imports: [],
  templateUrl: './leagues-page.html',
  styleUrl: './leagues-page.scss',
})
export class LeaguesPage implements OnInit {
  private readonly leagueApi = inject(LeagueApi);

  protected readonly leagues = signal<League[] | undefined>(undefined);

  ngOnInit(): void {
    this.loadLeagues();
  }

  private loadLeagues(): void {
    this.leagueApi.getLeagues().subscribe((leagues) => this.leagues.set(leagues));
  }
}
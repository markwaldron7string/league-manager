import { Routes } from '@angular/router';
import { LeaguesPage } from './features/leagues/leagues-page';

export const routes: Routes = [
    { path: '', redirectTo: 'leagues', pathMatch: 'full' },
    { path: 'leagues', component: LeaguesPage },
];

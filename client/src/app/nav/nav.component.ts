import { Component, OnDestroy, OnInit } from '@angular/core';
import { map, Observable, Subscription } from 'rxjs';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit, OnDestroy {

  model: any = {};
  currentUserName: string;
  currentUser: Observable<User>;
  accountSub: Subscription;

  constructor( private accountService: AccountService) { }


  ngOnInit(): void {
    this.currentUser = this.accountService.currentUser$;
    this.accountSub = this.accountService.currentUser$.subscribe(user => this.currentUserName = user.username);
  }

  login() {

    // console.log(this.model);

     this.accountService.login(this.model)
      .subscribe({
        next: response => {
        console.log(response);
      },
        error: error => console.log(error)
      });
  }

  logout() {
    this.accountService.logout();
  }

  ngOnDestroy(): void {
    this.accountSub.unsubscribe();
  }

}

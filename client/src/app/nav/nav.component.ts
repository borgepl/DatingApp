import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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

  constructor( private accountService: AccountService,
               private router: Router,
               private toastr: ToastrService
              ) { }


  ngOnInit(): void {
    this.currentUser = this.accountService.currentUser$;
    this.accountSub = this.accountService.currentUser$.subscribe(user => this.currentUserName = user.username);
  }

  login() {

    // console.log(this.model);

     this.accountService.login(this.model)
      .subscribe({
        next: response => {
        // console.log(response);
        this.router.navigateByUrl('/members');
      },
        error: error => {
          console.log(error);
          this.toastr.error(error.error);
      }
      });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  ngOnDestroy(): void {
    this.accountSub.unsubscribe();
  }

}

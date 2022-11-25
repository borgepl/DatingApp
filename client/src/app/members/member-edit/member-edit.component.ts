import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs';
import { Member } from 'src/app/models/member';
import { User } from 'src/app/models/user';
import { AccountService } from 'src/app/services/account.service';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {

  member: Member | undefined;
  user: User | undefined;

  constructor( private accountService: AccountService,
               private membersService : MembersService
              ) { this.accountService.currentUser$.pipe(take(1)).subscribe({
                  next: user => this.user = user
              }) }

  ngOnInit(): void {
    this.loadmember();
  }

  loadmember() {
    if (!this.user) return;
    this.membersService.getMember(this.user.username).subscribe({
      next: member => this.member = member
    })
  }

}

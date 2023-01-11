import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { User } from 'src/app/models/user';
import { AccountService } from 'src/app/services/account.service';
import { MembersService } from 'src/app/services/members.service';
import { MessageService } from 'src/app/services/message.service';
import { PresenceService } from 'src/app/services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {

  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent
  activeTab?: TabDirective;

  member: Member = {} as Member;
  user? : User;
  messages: Message[] = [];
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];

  constructor( private membersService: MembersService,
               private accountService: AccountService,
               private messageService: MessageService,
               public presenceService: PresenceService,
               private route: ActivatedRoute,
               private router: Router
              )
            {
                this.accountService.currentUser$.pipe(take(1)).subscribe({
                  next: (user) => {
                    if (user) this.user = user;
                  }
                });
                this.router.routeReuseStrategy.shouldReuseRoute = () => false;

            }

  ngOnInit(): void {

    // gets data from the route-resolver - added to the routing in the app-routing-module
    this.route.data.subscribe({
      next:  data => {
        this.member = data['member'];
        this.galleryImages = this.getImages();
      }
    })
    //this.loadMember();

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

  }

  getImages() {
    if (!this.member) return [];
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url
      })
    }
    return imageUrls;
  }

  loadMember() {
    this.membersService.getMember(this.route.snapshot.paramMap.get('username')).subscribe({
      next: member => {
        this.member = member;
        this.galleryImages = this.getImages();
      }
    })
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMessageThread(this.member.username).subscribe({
        next: messages => this.messages = messages
      });
    }

  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.user) {
      // load messages without SignalR
      // this.loadMessages();

      // with SignalR
      this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

ngOnDestroy(): void {
  this.messageService.stopHubConnection();
}

}

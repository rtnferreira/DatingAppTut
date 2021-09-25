import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { NgxGalleryImage, NgxGalleryOptions, NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {


  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[]= [];
  activeTab: TabDirective;

  member: Member;
  messages: Message[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute
              ,private messageService: MessageService) { }

  ngOnInit(): void
  {
    /* this.loadMember(); */
    this.route.data.subscribe(data =>
    {
        this.member = data.member;
    });

    this.route.queryParams.subscribe(params =>
    {
        params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

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

    this.galleryImages = this.getImages();
  }

  getImages(): NgxGalleryImage[]
  {
    const imageUrls = [];
    for(const photo of this.member.photos)
    {
      imageUrls.push(
        {
          small: photo?.url,
          medium: photo?.url,
          big: photo?.url,
        }
      );
    }

    return imageUrls;
  }


/*   loadMember()
  {
    let param: any = this.route.snapshot.paramMap.get('username');
    this.memberService.getMember(param)
                      .subscribe(member =>
                        {
                          this.member = member;
                          this.galleryImages = this.getImages();
                        });
  } */

  loadMessages()
  {
    this.messageService.getMessageThread(this.member.userName)
                        .subscribe(messages => {
                            this.messages = messages;
                        });
  }


   onTabActivated(data: TabDirective)
   {
     this.activeTab = data;

     if(this.activeTab.heading === 'Messages' && this.messages.length === 0)
     {
        this.loadMessages();
     }
   }

   selectTab(tabId: number)
   {
     this.memberTabs.tabs[tabId].active = true;
   }
}

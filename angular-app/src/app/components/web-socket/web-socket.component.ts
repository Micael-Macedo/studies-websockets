import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WebSocketService } from '../../core/services/websocket.service';
import { Subscription } from 'rxjs';
import { biometric } from '../../core/interfaces/biometric';

@Component({
  selector: 'web-socket',
  imports: [CommonModule],
  templateUrl: './web-socket.component.html',
  styleUrls: ['./web-socket.component.css'],
})
export class WebSocketComponent implements OnInit, OnDestroy {
  message!: biometric;
  showDiv: boolean = false;
  private socketSubscription!: Subscription;

  constructor(private wsService: WebSocketService){}


  ngOnInit() {
    this.socketSubscription = this.wsService.getMessages().subscribe((msg) => {
      this.message = msg;
      this.toggleDiv();
    });
  }

  sendMessage() {
    this.wsService.requestImage();
  }

  toggleDiv() {
    this.showDiv = !this.showDiv;
  }

  ngOnDestroy() {
    this.socketSubscription.unsubscribe();
    this.wsService.close();
  }
}

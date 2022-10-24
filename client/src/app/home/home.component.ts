import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  registerMode = false;
  users: any;
  api = 'https://localhost:5001/api/users';

  constructor( private http: HttpClient) { }

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  getUsers() {
   this.http.get(this.api).subscribe({
    next: (users) => this.users = users,
    error: (error) => console.log(error)
    });
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}

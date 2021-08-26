import { Component, Input, OnInit, EventEmitter, Output } from '@angular/core';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Output() cancelRegister = new EventEmitter();
  model: any = {};

  constructor(private accountService: AccountService, private toastr: ToastrService) { }

  ngOnInit(): void
  {

  }

  register()
  {
    /* console.log(this.model); */
    this.accountService.register(this.model).subscribe(response =>
      {
        console.log(response);
        this.cancel();
      },
      error =>
      {
        this.toastr.error(error.error);
      })
  }

  cancel()
  {
    this.cancelRegister.emit(false);
  }

}

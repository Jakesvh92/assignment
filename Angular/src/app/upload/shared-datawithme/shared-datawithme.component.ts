import { Component, OnInit } from '@angular/core';
import { UserService } from './../../shared/user.service';

@Component({
  selector: 'app-shared-datawithme',
  templateUrl: './shared-datawithme.component.html',
  styleUrls: ['./shared-datawithme.component.css']
})
export class SharedDatawithmeComponent implements OnInit {
  dataList;
  dtOptions: DataTables.Settings = {};
  displayTable: boolean = false;
  constructor(private serviceuser: UserService) { }

  ngOnInit() {
    this.dtOptions = {
      pagingType: 'full_numbers',
      pageLength: 5,
      processing: true
    };
    var userid =  localStorage.getItem('userid');
    this.serviceuser.getAll(userid).subscribe(
      (res: any) => {
        if (res.listData) {
          this.dtOptions = res.listData;
        this.displayTable = true;
          this.dataList = res.listData;
        }
      },
      err => {
        console.log(err);
      }
    );
  }


}

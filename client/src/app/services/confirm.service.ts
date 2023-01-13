import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { map, Observable, of, take } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {

  bsModalRef: BsModalRef<ConfirmDialogComponent>;
  result: boolean;

  constructor( private modalService: BsModalService) { }

  confirm(
    title = 'Confirmation',
    message = 'Are you sure you want to do this? All unsaved changes will be lost!',
    btnOkText = 'Ok',
    btnCancelText = 'Cancel'
  ) : Observable<boolean> {
    const config = {
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModalRef = this.modalService.show(ConfirmDialogComponent,config);
    return this.bsModalRef.onHidden.pipe(
      map(() => {
        console.log('------- ' + this.bsModalRef.content.result);
        return this.bsModalRef.content.result;
      })
    );
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterModule } from '@angular/router';

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Step {
  number: number;
  title: string;
  description: string;
  icon: string;
}

interface FloatingIcon {
  icon: string;
  top: string;
  left: string;
  delay: number;
  duration: number;
}

@Component({
  selector: 'app-home',
  templateUrl: 'home.component.html',
  styleUrls: ['home.component.scss'],
  imports: [MatIconModule, MatButtonModule, CommonModule, RouterModule]
})

export class HomeComponent implements OnInit {
  constructor(private router: Router) {}

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  floatingIcons: FloatingIcon[] = [
    { icon: 'verified_user', top: '10%', left: '10%', delay: 0, duration: 3 },
    { icon: 'description', top: '20%', left: '85%', delay: 0.5, duration: 3.5 },
    { icon: 'draw', top: '60%', left: '5%', delay: 1, duration: 4 },
    { icon: 'cloud_done', top: '70%', left: '90%', delay: 1.5, duration: 3.2 },
    { icon: 'security', top: '40%', left: '15%', delay: 0.8, duration: 3.8 },
    { icon: 'check_circle', top: '80%', left: '80%', delay: 1.2, duration: 3.3 },
  ];

  features: Feature[] = [
    {
      icon: 'verified_user',
      title: 'Segurança ICP-Brasil',
      description: 'Assinaturas digitais com validade jurídica garantida através da certificação ICP-Brasil, garantindo a autenticidade e integridade dos documentos.'
    },
    {
      icon: 'speed',
      title: 'Agilidade e Eficiência',
      description: 'Processo de assinatura digital rápido e eficiente, eliminando a necessidade de impressão, envio físico e armazenamento de documentos em papel.'
    },
    {
      icon: 'cloud_done',
      title: 'Armazenamento Seguro',
      description: 'Documentos armazenados em nuvem com criptografia de ponta a ponta.'
    },
    {
      icon: 'timeline',
      title: 'Rastreabilidade',
      description: 'Auditoria completa com registros de data, hora e dispositivo de cada assinatura.'
    },
    {
      icon: 'notifications_active',
      title: 'Notificações Automáticas',
      description: 'Receba alertas e atualizações por e-mail em tempo real sobre o status das suas assinaturas.'
    },
    {
      icon: 'policy',
      title: 'Conformidade LGPD',
      description: 'Proteção de dados pessoais em conformidade com a Lei Geral de Proteção de Dados (LGPD), garantindo a privacidade e segurança dos usuários.'
    }
  ];

  steps: Step[] = [
    {
      number: 1,
      title: 'Upload do Documento',
      description: 'Faça o upload do documento que precisa ser assinado',
      icon: 'cloud_upload'
    },
    {
      number: 2,
      title: 'Defina os Signatários',
      description: 'Adicione os signatários e configure a ordem de assinatura',
      icon: 'people'
    },
    {
      number: 3,
      title: 'Envie para Assinatura',
      description: 'Os signatários receberão notificações por e-mail',
      icon: 'send'
    },
    {
      number: 4,
      title: 'Acompanhe o Processo',
      description: 'Monitore o status em tempo real até a conclusão',
      icon: 'track_changes'
    }
  ];

  ngOnInit(): void {}
}

import { CommonModule } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatExpansionModule } from "@angular/material/expansion";
import {MatFormFieldModule} from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';

interface FaqCategory {
  name: string;
  icon: string;
  questions: FaqQuestion[];
}

interface FaqQuestion {
  question: string;
  answer: string;
}

@Component({
  selector: "app-faq",
  templateUrl: "./faq.component.html",
  styleUrls: ["./faq.component.scss"],
  imports: [
    MatIconModule,
    MatButtonModule,
    CommonModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
  ]
})
export class FaqComponent implements OnInit {
  selectedCategory = 0;
  searchQuery = "";

  faqCategories: FaqCategory[] = [
    {
      name: "Geral",
      icon: "help_outline",
      questions: [
        {
          question: "O que é o SignatureHub?",
          answer: "O SignatureHub é a plataforma oficial da Advocacia-Geral do Estado de Minas Gerais (AGE-MG) para gerenciamento de assinaturas digitais. Permite que documentos sejam assinados eletronicamente ou com certificado digital ICP-Brasil, com total validade jurídica."
        },
        {
          question: "As assinaturas tem validade jurídica?",
          answer: "Sim! As assinaturas digitais realizadas através do SignatureHub com certificado ICP-Brasil possuem validade jurídica nos termos da Medida Provisória 2.200-2/2001 e da Lei 14.063/2020. Assinaturas eletrônicas simples também são válidas conforme previsto na legislação."
        },
        {
          question: "Quem pode usar o sistema?",
          answer: 'O sistema é destinado aos servidores e colaboradores da AGE-MG. Para ter acesso, é necessário possuir credenciais fornecidas pelo departamento de TI da instituição.'
        },
        {
          question: "O sistema está disponível 24/7?",
          answer: "Sim, o SignatureHub está disponível para uso a qualquer momento. No entanto, recomendamos verificar os horários de manutenção programada que podem afetar temporariamente o acesso ao sistema."
        }
      ]
    },
    {
      name: "Assinatura Digital",
      icon: "draw",
      questions: [
        {
          question: 'Qual a diferença entre assinatura eletrônica e digital?',
          answer: 'Assinatura eletrônica simples é feita através de PIN ou OTP e tem validade legal. Assinatura digital utiliza certificado ICP-Brasil (A1 ou A3), possui criptografia e é considerada equivalente à assinatura de próprio punho com presunção de autenticidade.'
        },
        {
          question: 'Como assinar um documento?',
          answer: 'Você receberá um e-mail com o link para acessar o documento. Ao clicar, poderá escolher entre assinatura eletrônica (com PIN) ou assinatura digital (com seu certificado ICP-Brasil). Após assinar, o documento é automaticamente atualizado e os demais signatários são notificados.'
        },
        {
          question: 'Posso assinar pelo celular?',
          answer: 'Sim! O sistema é responsivo e pode ser acessado através de qualquer dispositivo com navegador web. Para assinatura com certificado A3 em smartphone, é necessário utilizar token compatível com NFC ou adaptador OTG.'
        },
        {
          question: 'Preciso ter certificado digital?',
          answer: 'Não necessariamente. Você pode optar por assinatura eletrônica simples, que é válida legalmente. No entanto, para documentos que exigem maior segurança e validade jurídica, recomendamos o uso de certificado digital ICP-Brasil.'
        },
        {
          question: 'O que é certificado digital ICP-Brasil?',
          answer: 'Certificado digital ICP-Brasil é um arquivo eletrônico que comprova a identidade de uma pessoa ou entidade no ambiente digital. Ele é emitido por Autoridades Certificadoras credenciadas e pode ser do tipo A1 (armazenado no computador) ou A3 (armazenado em dispositivo físico como token ou smart card).'
        },
        {
          question: 'O que é assinatura eletrônica simples?',
          answer: 'Assinatura eletrônica simples é um método de assinatura que não utiliza certificado digital, mas sim um código PIN ou OTP enviado por e-mail ou SMS. Ela é válida legalmente para a maioria dos documentos, exceto aqueles que exigem maior segurança ou validade jurídica, onde o uso de certificado digital é recomendado.'
        },
      ]
    },
    {
      name: "Documentos",
      icon: "description",
      questions: [
        {
          question: 'Quais formatos de arquivo são aceitos?',
          answer: 'O sistema aceita documentos em formato PDF. Outros formatos como DOC, DOCX podem ser convertidos para PDF antes do upload.'
        },
        {
          question: 'Qual o tamanho máximo de arquivo?',
          answer: 'O tamanho máximo por documento é de 50MB. Para arquivos maiores, recomenda-se compactar ou dividir o documento.'
        },
        {
          question: 'Como funciona o fluxo de assinatura?',
          answer: 'Você pode configurar três tipos de fluxo: Sequencial (assinaturas em ordem definida), Paralelo (todos assinam simultaneamente) ou Híbrido (combinação dos dois). O sistema notifica automaticamente cada signatário quando chegar sua vez.'
        },
        {
          question: 'Por quanto tempo os documentos ficam armazenados?',
          answer: 'Os documentos são armazenados de forma permanente no sistema, com backup diário. O acesso aos documentos antigos pode ser feito a qualquer momento através do histórico.'
        }
      ]
    },
    {
      name: "Segurança",
      icon: "security",
      questions: [
        {
          question: 'Como meus documentos são protegidos?',
          answer: 'Todos os documentos são criptografados com AES-256 em repouso e TLS 1.3 durante a transmissão. O acesso é controlado por autenticação forte e os dados são armazenados em servidores seguros com backup redundante.'
        },
        {
          question: 'O sistema está em conformidade com a LGPD?',
          answer: 'Sim, o SignatureHub foi desenvolvido em total conformidade com a Lei Geral de Proteção de Dados (LGPD). Coletamos apenas os dados necessários, com consentimento, e você tem direito de acessar, corrigir ou excluir suas informações a qualquer momento.'
        },
        {
          question: 'Quem pode ver meus documentos?',
          answer: 'Apenas os signatários definidos e o criador do documento têm acesso. Administradores do sistema têm acesso apenas para fins de suporte técnico e auditoria, sempre registrado em log.'
        },
        {
          question: 'O que acontece se eu perder meu certificado digital?',
          answer: 'Se você perder seu certificado digital, deve entrar em contato imediatamente com a Autoridade Certificadora que o emitiu para revogá-lo. No SignatureHub, recomendamos também atualizar suas credenciais de acesso para garantir a segurança de seus documentos.'
        },
        {
          question: 'Como funciona a auditoria?',
          answer: 'O sistema mantém um registro detalhado de todas as ações realizadas, incluindo quem assinou, quando e qual documento foi assinado. Esses logs são armazenados de forma segura e podem ser acessados para fins de auditoria ou resolução de disputas.'
        }
      ]
    },
    {
      name: "Suporte",
      icon: "support_agent",
      questions: [
        {
          question: 'Como posso entrar em contato com o suporte?',
          answer: 'Você pode entrar em contato através do e-mail desenvolvimento@advocaciageral.mg.gov.br ou pelo ramal 825, de segunda a sexta-feira, das 7:30h às 16:30h.'
        },
        {
          question: 'Quanto tempo leva para receber uma resposta?',
          answer: 'Nos esforçamos para responder a todas as solicitações de suporte dentro de 24 horas úteis. Em casos urgentes, recomendamos entrar em contato por telefone.'
        },
        {
          question: 'O que fazer se esquecer minha senha?',
          answer: 'Se você esquecer sua senha, clique em "Esqueci minha senha" na tela de login e siga as instruções para redefini-la.'
        }
      ]
    }
  ];

  get filteredQuestions(): FaqQuestion[] {
    if (!this.searchQuery.trim()) {
      return this.faqCategories[this.selectedCategory].questions;
    }

    const query = this.searchQuery.toLowerCase();
    const allQuestions: FaqQuestion[] = [];

    this.faqCategories.forEach(category => {
      const filtered = category.questions.filter(q =>
        q.question.toLowerCase().includes(query) ||
        q.answer.toLowerCase().includes(query)
      );
      allQuestions.push(...filtered);
    });
    return allQuestions;
  }

  ngOnInit(): void {
    // Initialize FAQ categories here
  }

  selectCategory(index: number): void {
    this.selectedCategory = index;
    this.searchQuery = ""; // Clear search when changing category
  }

  clearSearch(): void {
    this.searchQuery = "";
  }
}
